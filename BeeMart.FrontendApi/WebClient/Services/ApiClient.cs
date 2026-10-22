using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;

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
    public async Task<(int Status, string Body)> ForwardAsync(HttpMethod method, string url, string token, string? json = null)
    {
        try
        {
            var req = new HttpRequestMessage(method, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (json != null)
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            return ((int)res.StatusCode, await res.Content.ReadAsStringAsync());
        }
        catch (HttpRequestException)
        {
            return (503, "{\"message\":\"Tidak dapat terhubung ke server API.\"}");
        }
    }
}

    