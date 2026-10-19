using System.Security.Claims;
using Asp.Versioning;
using BeeMart.BackendApi.Data;
using BeeMart.BackendApi.Dtos;
using BeeMart.BackendApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.BackendApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/transactions")]
[Authorize(Roles = "Admin,Cashier")]
public class TransactionsController : ControllerBase
{
    private readonly BeeMartDbContext _db;

    public TransactionsController(BeeMartDbContext db)
    {
        _db = db;
    }

    private static readonly TimeZoneInfo Wib = GetWib();

    private static TimeZoneInfo GetWib()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }

    // Menghitung nomor invoice 
    private async Task<string> BuildNextInvoiceAsync(string idUser)
    {
        var nowWib = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Wib);
        var startWib = nowWib.Date;
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startWib, Wib);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(startWib.AddDays(1), Wib);

        var count = await _db.TrTransactions
            .CountAsync(t => t.TransactionDate >= startUtc && t.TransactionDate < endUtc);

        return idUser + nowWib.ToString("ddMMyy") + (count + 1).ToString("D3");
    }

    [HttpGet("next-invoice")]
    public async Task<IActionResult> NextInvoice()
    {
        var idUser = User.FindFirstValue("idUser")!;
        return Ok(new { invoiceNo = await BuildNextInvoiceAsync(idUser) });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest req)
    {
        var idUser = User.FindFirstValue("idUser")!;

 
        if (req.Items == null || req.Items.Count == 0)
            return BadRequest(new { message = "Keranjang kosong." });

        if (req.Items.Any(i => string.IsNullOrWhiteSpace(i.IdProduct) || i.Qty <= 0))
            return BadRequest(new { message = "Item tidak valid. Qty harus lebih dari 0." });

        var payment = await _db.LtPaymentMethods
            .FirstOrDefaultAsync(p => p.IdPaymentMethod == req.IdPaymentMethod && p.Stsrc);
        if (payment == null)
            return BadRequest(new { message = "Metode pembayaran tidak valid." });

        // 2. Gabungkan produk 
        var grouped = req.Items
            .GroupBy(i => i.IdProduct!)
            .Select(g => new { IdProduct = g.Key, Qty = g.Sum(x => x.Qty) })
            .ToList();

        await using var trx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        // 3. Ambil produk dari DB 
        var ids = grouped.Select(g => g.IdProduct).ToList();
        var products = await _db.MsProducts
            .Where(p => ids.Contains(p.IdProduct) && p.Stsrc)
            .ToDictionaryAsync(p => p.IdProduct);

        var details = new List<TrTransactionDetail>();
        decimal total = 0;
        var idTransaction = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        foreach (var g in grouped)
        {
            if (!products.TryGetValue(g.IdProduct, out var p))
                return BadRequest(new { message = "Produk tidak ditemukan atau tidak aktif." });

            if (g.Qty > p.Stock)
                return BadRequest(new { message = "Jumlah barang yang dimasukkan melebihi sisa stok." });

            var subTotal = g.Qty * p.Price * (1 - p.Discount / 100m);
            total += subTotal;

            p.Stock -= g.Qty;
            p.Dateup = now;
            p.Userup = idUser;

            details.Add(new TrTransactionDetail
            {
                IdTransactionDetail = Guid.NewGuid().ToString(),
                IdTransaction = idTransaction,
                IdProduct = p.IdProduct,
                Qty = g.Qty,
                Price = p.Price,
                Discount = p.Discount,
                SubTotal = subTotal,
                Stsrc = true,
                Datein = now,
                Userin = idUser
            });
        }

        // 4. Buat invoice 
        var invoiceNo = await BuildNextInvoiceAsync(idUser);

        var header = new TrTransaction
        {
            IdTransaction = idTransaction,
            InvoiceNo = invoiceNo,
            TransactionDate = now,
            IdUser = idUser,
            IdPaymentMethod = payment.IdPaymentMethod,
            TotalPrice = total,
            Stsrc = true,
            Datein = now,
            Userin = idUser
        };

        _db.TrTransactions.Add(header);
        _db.TrTransactionDetails.AddRange(details);

        await _db.SaveChangesAsync();
        await trx.CommitAsync();

        return StatusCode(201, new
        {
            idTransaction = header.IdTransaction,
            invoiceNo = header.InvoiceNo,
            transactionDate = header.TransactionDate,
            totalPrice = header.TotalPrice
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> History([FromQuery] string? date, [FromQuery] int page = 1)
    {
        const int pageSize = 20;

        if (string.IsNullOrWhiteSpace(date)
            || !DateTime.TryParseExact(date, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var day))
        {
            return BadRequest(new
            {
                message = "Tanggal wajib diisi dengan format yyyy-MM-dd.",
                errors = new Dictionary<string, string[]> { ["date"] = new[] { "Tanggal wajib dipilih." } }
            });
        }

        if (page < 1) page = 1;

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(day.Date, DateTimeKind.Unspecified), Wib);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(day.Date.AddDays(1), DateTimeKind.Unspecified), Wib);

        var query = _db.TrTransactions
            .Where(t => t.TransactionDate >= startUtc && t.TransactionDate < endUtc);

        // Hak akses ditentukan dari token: Kasir hanya datanya sendiri
        if (!User.IsInRole("Admin"))
        {
            var idUser = User.FindFirstValue("idUser")!;
            query = query.Where(t => t.IdUser == idUser);
        }

        var totalItems = await query.CountAsync();

        var rows = await query
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.InvoiceNo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.InvoiceNo,
                t.TransactionDate,
                CashierName = t.IdUserNavigation.FullName,
                TotalItems = t.TrTransactionDetails.Sum(d => d.Qty),
                PaymentMethod = t.IdPaymentMethodNavigation.MethodName,
                t.TotalPrice
            })
            .ToListAsync();

        var items = rows.Select(r => new
        {
            invoiceNo = r.InvoiceNo,
            transactionDate = DateTime.SpecifyKind(r.TransactionDate, DateTimeKind.Utc),
            cashierName = r.CashierName,
            totalItems = r.TotalItems,
            paymentMethod = r.PaymentMethod,
            totalPrice = r.TotalPrice
        });

        return Ok(new
        {
            items,
            page,
            pageSize,
            totalItems,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        });
    }
}