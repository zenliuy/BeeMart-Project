using BeeMart.WebClient.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Controllers;

[Authorize]
[Route("ui")]
public class ProxyController : Controller
{
    private readonly ApiClient _api;

    public ProxyController(ApiClient api)
    {
        _api = api;
    }

    private string? Token => User.FindFirst("token")?.Value;

    private async Task<IActionResult> Forward(HttpMethod method, string url, string? json = null)
    {
        var token = Token;
        if (string.IsNullOrWhiteSpace(token))
        {
            return new JsonResult(new { message = "Sesi login telah berakhir. Silakan login kembali." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        var (status, body) = await _api.ForwardAsync(method, url, token, json);
        return new ContentResult { Content = body, ContentType = "application/json", StatusCode = status };
    }

    [HttpGet("products")]
    public Task<IActionResult> Products(string? search) =>
        Forward(HttpMethod.Get, $"api/v1/products?search={Uri.EscapeDataString(search ?? "")}");

    [HttpGet("payment-methods")]
    public Task<IActionResult> PaymentMethods() =>
        Forward(HttpMethod.Get, "api/v1/payment-methods");

    [HttpGet("next-invoice")]
    public Task<IActionResult> NextInvoice() =>
        Forward(HttpMethod.Get, "api/v1/transactions/next-invoice");

    [HttpPost("submit")]
    public async Task<IActionResult> Submit()
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();
        return await Forward(HttpMethod.Post, "api/v1/transactions", json);
    }

    [HttpGet("history")]
    public Task<IActionResult> History(string? date, int page = 1) =>
        Forward(HttpMethod.Get,
            $"api/v1/transactions?date={Uri.EscapeDataString(date ?? "")}&page={page}");
}