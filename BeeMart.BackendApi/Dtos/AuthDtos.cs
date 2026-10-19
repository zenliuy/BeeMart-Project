namespace BeeMart.BackendApi.Dtos;

public class RegisterRequest
{
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Password { get; set; }
    public string? RePassword { get; set; }
    public string? Role { get; set; }
}

public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
}
