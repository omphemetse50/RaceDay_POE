using Microsoft.AspNetCore.Authentication.Cookies;
using RaceDay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC ----------
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ---------- Cookie authentication ----------
// This IS the "session management to maintain the user's authenticated state and
// role across all pages" the Part 3 brief asks for. ASP.NET Core's cookie auth
// middleware handles persisting the signed-in principal (UserId, Role, FullName,
// plus the API's own session cookie as a claim) across requests via a secure,
// HttpOnly cookie on this app's own domain -- completely separate from the API's
// session cookie, which never reaches the browser.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // from 2 Hours
        options.SlidingExpiration = true;
        options.Cookie.Name = "RaceDay.Web.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

// ---------- RaceDay API clients ----------
var apiBaseUrl = builder.Configuration["RaceDayApi:BaseUrl"]
    ?? throw new InvalidOperationException("RaceDayApi:BaseUrl is not configured.");

// Used only for Register/Login/Logout, before a session principal exists.
builder.Services.AddHttpClient<AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

builder.Services.AddTransient<ApiSessionCookieHandler>();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false }) // <-- CRITICAL
.AddHttpMessageHandler<ApiSessionCookieHandler>();

var app = builder.Build();

// ---------- Middleware pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
