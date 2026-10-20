namespace BeeMart.WebClient.Models;

public class RegisterViewModel
{
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Password { get; set; }
    public string? RePassword { get; set; }
    public string Role { get; set; } = "Cashier";
    public string? Error { get; set; }
}