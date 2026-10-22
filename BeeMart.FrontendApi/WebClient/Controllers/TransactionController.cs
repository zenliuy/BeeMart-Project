using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Controllers;

[Authorize]
public class TransactionController : Controller
{
    public IActionResult New() => View();
    public IActionResult History() => View();
}