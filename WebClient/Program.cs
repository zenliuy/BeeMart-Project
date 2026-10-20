using BeeMart.WebClient.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<ApiClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!));

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(o =>
        {
            o.LoginPath = "/Account/Login";
            o.Cookie.HttpOnly = true;
            o.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        });

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
