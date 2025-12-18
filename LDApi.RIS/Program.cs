using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
using LDApi.RIS.Providers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using LDApi.RIS.Data;
using LDApi.RIS.Models;

var builder = WebApplication.CreateBuilder(args);

// dotnet test (host test)
var isTesting = builder.Environment.EnvironmentName == "Development" &&
                AppDomain.CurrentDomain.FriendlyName.Contains("test", StringComparison.OrdinalIgnoreCase);


var isIntegrationTest =
    builder.Environment.IsEnvironment("Test") ||
    AppDomain.CurrentDomain.FriendlyName.Contains("test", StringComparison.OrdinalIgnoreCase);
// --- Services DI ---

builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IHL7Service, HL7Service>();
builder.Services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<IGuidProvider, SystemGuidProvider>();
builder.Services.AddScoped<IMllpClientService, MllpClientService>();
builder.Services.AddScoped<IReportYamlService, ReportYamlService>();

builder.Services.AddSingleton<ConfigurationService>();
builder.Services.AddScoped<HL7Service>();
builder.Services.AddScoped<ReportYamlService>();

// --- EF Core + Identity ---

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5; // maximum 5 tentatives
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // lockout au bout de 15'
    }
)
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// --- Authentication JWT ---

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
});

// --- Controllers + JSON options ---

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// --- CORS pour le front React ---

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("*")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Apply migrations at startup
// 1. Appliquer les migrations
if (!isIntegrationTest)
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        db.Database.Migrate();
    }

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await IdentitySeeder.SeedAsync(services);
    }
}
// gestion des roles (user et administrateur)

static async Task SeedRolesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = new[] { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}
if (!isIntegrationTest)
{
    await SeedRolesAsync(app.Services);
}



app.UseRouting();

app.UseCors("AllowReact");

app.UseAuthentication();
app.UseAuthorization();

// Servir le front React + fichiers statiques, seulement si nécessaire


if (!isTesting)
{
    var uiPath = Path.Combine(Directory.GetCurrentDirectory(), "../wwwroot");
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(uiPath)
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uiPath),
        RequestPath = ""
    });

    // fallback pour le SPA : toutes les routes non-API -> index.html
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uiPath)
    });
}

app.MapControllers();

app.Run("http://0.0.0.0:5033");

// Pour les tests d’intégration si besoin
public partial class Program { }
