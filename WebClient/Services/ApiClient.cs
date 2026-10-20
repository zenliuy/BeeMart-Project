using System.Net;
using System.Net.Http.Json;

namespace BeeMart.WebClient.Services;

public class UserInfo
{
    public string IdUser { get; set;} = "";
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
}

public class LoginResult
{
    public string Token { get; set; } = "";
    public UserInfo User { get; set; } = new();
}

public class ErrorResponse
{
    public string? Message { get; set; }
}

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<(LoginResult? Result, string? Error)> LoginAsync(string username, string password, string role)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/v1/auth/login", new { username, password, role });
            if (res.IsSuccessStatusCode)
                return (await res.Content.ReadFromJsonAsync<LoginResult>(), null);

            if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
            {
                var err = await res.Content.ReadFromJsonAsync<ErrorResponse>();
                return (null, err?.Message ?? "Invalid login");
            }

            return (null, "Terjadi kesalahan pada server.");
        }
        catch (HttpRequestException)
        {
            return (null, "Tidak dapat terhubung ke server API.");
        }
    }
    public async Task<string?> RegisterAsync(string username, string fullName, string password, string rePassword, string role)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/v1/auth/register",
                new { username, fullName, password, rePassword, role });

            if (res.IsSuccessStatusCode) return null;

            if (res.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest)
            {
                var err = await res.Content.ReadFromJsonAsync<ErrorResponse>();
                return err?.Message ?? "Registrasi gagal.";
            }

            return "Terjadi kesalahan pada server.";
        }
        catch (HttpRequestException)
        {
            return "Tidak dapat terhubung ke server API.";
        }
    }
}

    