using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("New", "Transaction");
}


