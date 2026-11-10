using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using RaceDay.Authentication;
using RaceDay.Common;
using RaceDay.Controllers;
using RaceDay.Database;
using RaceDay.DTOs;
using RaceDay.Extensions;
using RaceDay.Models;
using RaceDay.Services;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

// Database (EF Core, Code-First against SQL Server)

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<RaceDayDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// ---------- Controllers ----------
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// ---------- Server-side session ----------
// Session data (UserId, Role, Email, FullName) is stored server-side (in-memory cache
// here; swap AddDistributedMemoryCache for AddStackExchangeRedisCache etc. in production
// / when running multiple API instances behind a load balancer). The client only ever
// holds an opaque, HttpOnly session-ID cookie -- it cannot read or forge the role itself.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "RaceDay.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ---------- Authentication ----------
// A custom scheme reads the authenticated identity out of session state on every
// request, so [Authorize] / [Authorize(Roles = "Organiser")] work exactly as they
// would with any other ASP.NET Core auth handler.
builder.Services.AddAuthentication(SessionAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, SessionAuthenticationHandler>(SessionAuthenticationHandler.SchemeName, options => { });

builder.Services.AddAuthorization();

// ---------- Azure Blob Storage ----------
// Part 3 requirement: all image uploads (profile pictures, event banners) are
// stored in Azure Blob Storage. The MVC app never talks to Azure directly -- it
// posts the file to this API's existing upload endpoints, and this service is
// the only thing that touches Azure. ConnectionStrings:AzureBlobStorage defaults
// to the Azurite local emulator ("UseDevelopmentStorage=true") in appsettings.json;
// swap it for a real Azure Storage account connection string in production.
builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay event management platform (Part 2)."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // ADD THIS: Tells Swagger to ignore IFormFile properties
    options.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
});

// ---------- CORS ----------
// Session cookies require explicit origins + AllowCredentials (wildcard origins are
// rejected by browsers once credentials are involved).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvcClient", policy =>
    {
        policy
            .WithOrigins("https://localhost:7100", "http://localhost:5100")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ---------- Middleware pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
});

app.UseHttpsRedirection();
app.UseStaticFiles(); // serves wwwroot/uploads (profile pictures, event banners)

app.UseCors("AllowMvcClient");

// Session MUST be registered before Authentication/Authorization -- the session
// authentication handler reads Context.Session, which only exists once this runs.
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---------- Apply EF Core migrations automatically at startup ----------
// The published Docker runtime image doesn't carry the `dotnet ef` CLI tools
// (those are SDK-only), so there's no way to run `dotnet ef database update`
// inside the running container. Applying pending migrations here instead means
// `docker compose up` alone is enough to get a fully working, schema-correct
// database -- no manual migration step required. Skipped under "Testing" since
// RaceDayApiFactory (in RaceDay.Tests) uses EF Core's InMemory provider, which
// doesn't support migrations at all.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    db.Database.Migrate();
}

app.Run();

// Exposed so RaceDay.Tests can spin up an in-memory instance of the app.
public partial class Program { }
