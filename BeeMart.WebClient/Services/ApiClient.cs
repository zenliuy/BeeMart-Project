using System.Net;
using System.Net.Http.Json;

namespace BeeMart.WebClient.Services;

public class UserInfo
{
    public string IdUser { get; set; } = "";
    public string Username { get; set; } = "";
    public string Fullname { get; set; } = "";
    public string Role { get; set; } = "";
}

public class LoginResult;