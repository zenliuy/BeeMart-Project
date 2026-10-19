using Asp.Versioning;
using BeeMart.BackendApi.Data;
using BeeMart.BackendApi.Dtos;
using BeeMart.BackendApi.Models;
using BeeMart.BackendApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.BackendApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly BeeMartDbContext _db;
    private readonly TokenService _tokens;

    public AuthController(BeeMartDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var username = req.Username?.Trim();
        var fullName = req.FullName?.Trim();

        // 1. Validasi input
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(username)) errors["username"] = new[] { "Username wajib diisi." };
        else if (username.Length > 50) errors["username"] = new[] { "Username maksimal 50 karakter." };
            
        if (string.IsNullOrWhiteSpace(fullName)) errors["fullName"] = new[] { "Full Name wajib diisi." };
        else if (fullName.Length > 300) errors["fullName"] = new[] { "Full Name maksimal 300 karakter." };

        if (string.IsNullOrEmpty(req.Password)) errors["password"] = new[] { "Password wajib diisi." };
        if (string.IsNullOrEmpty(req.RePassword)) errors["rePassword"] = new[] { "Re-Password wajib diisi." };
        else if (req.Password != req.RePassword) errors["rePassword"] = new[] { "Re-Password harus sama dengan Password." };

        if (req.Role != "Admin" && req.Role != "Cashier") errors["role"] = new[] { "Role harus Admin atau Cashier." };

        if (errors.Count > 0)
            return BadRequest(new { message = "Validasi gagal.", errors });
            
        // 2. Cek username sudah dipakai
        if (await _db.MsUsers.AnyAsync(u => u.Username == username))
            return Conflict(new { message = "username has already exists" });

        // 3. Buat IdUser 5 digit 
        string idUser;
        do
        {
            idUser = Random.Shared.Next(0, 100000).ToString("D5");
        } while (await _db.MsUsers.AnyAsync(u => u.IdUser == idUser));

        // 4. password yang sudah di-hash
        var user = new MsUser
        {
            IdUser = idUser,
            Username = username!,
            FullName = fullName!,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = req.Role!,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = idUser
        };

        _db.MsUsers.Add(user);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Dua orang daftar dengan username sama di detik yang sama
            if (await _db.MsUsers.AnyAsync(u => u.Username == username))
                return Conflict(new { message = "username has already exists" });
            throw;
        }

        return StatusCode(201, new
        {
            idUser = user.IdUser,
            username = user.Username,
            fullName = user.FullName,
            role = user.Role
        });
     }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var username = req.Username?.Trim();

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(username)) errors["username"] = new[] { "Username wajib diisi." };
        if (string.IsNullOrEmpty(req.Password)) errors["password"] = new[] { "Password wajib diisi." };
        if (errors.Count > 0)
            return BadRequest(new { message = "Validasi gagal.", errors });

        var user = await _db.MsUsers
            .FirstOrDefaultAsync(u => u.Username == username && u.Stsrc);

        // Username, password, atau role salah: pesan SAMA
        if (user == null
            || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)
            || user.Role != req.Role)
        {
            return Unauthorized(new { message = "Invalid login" });
        }

        var (token, expiresAt) = _tokens.CreateToken(user);

        return Ok(new
        {
            token,
            expiresAt,
            user = new
            {
                idUser = user.IdUser,
                username = user.Username,
                fullName = user.FullName,
                role = user.Role
            }
        });
    }
}
