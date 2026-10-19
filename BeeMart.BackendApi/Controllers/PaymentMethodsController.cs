using Asp.Versioning;
using BeeMart.BackendApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.BackendApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly BeeMartDbContext _db;

    public PaymentMethodsController(BeeMartDbContext db)
    {  
        _db = db; 
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _db.LtPaymentMethods
            .Where(p => p.Stsrc)
            .OrderBy(p => p.IdPaymentMethod)
            .Select(p => new { idPaymentMethod = p.IdPaymentMethod, methodName = p.MethodName })
            .ToListAsync();

        return Ok(data);
    }           

}
      
