using Asp.Versioning;
using BeeMart.BackendApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.BackendApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[Authorize(Roles = "Admin,Cashier")]
public class ProductsController : ControllerBase
{
    private readonly BeeMartDbContext _db;

    public ProductsController(BeeMartDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search)
    {
        var keyword = search?.Trim() ?? "";

        var data = await _db.MsProducts
            .Where(p => p.Stsrc && p.Name.Contains(keyword))
            .OrderBy(p => p.Name)
            .Take(10)
            .Select(p => new
            {
                idProduct = p.IdProduct,
                name = p.Name,
                weight = p.Weight,
                stock = p.Stock,
                price = p.Price,
                discount = p.Discount
            })
            .ToListAsync();

        return Ok(data);
    }
}