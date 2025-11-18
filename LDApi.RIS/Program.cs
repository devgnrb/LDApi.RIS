using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
using LDApi.RIS.Providers;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Ajout des services
// -------------------------
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IHL7Service, HL7Service>();
builder.Services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<IGuidProvider, SystemGuidProvider>();

builder.Services.AddSingleton<IMllpConfigurationService, MllpConfigurationService>();
builder.Services.AddScoped<IMllpClientService>(sp =>
{
    var configService = sp.GetRequiredService<IMllpConfigurationService>();
    return new MllpClientService(configService.Host, configService.Port);
});
builder.Services.AddScoped<HL7Service>();

// Active les contrôleurs API
builder.Services.AddControllers();

// -------------------------
// CORS pour ton front React
// -------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// -------------------------
// Middleware
// -------------------------
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowReact");

// Mappe les contrôleurs
app.MapControllers();

// Fallback : toutes les routes non-API redirigent vers index.html
app.MapFallbackToFile("index.html");

// -------------------------
// Lancement de l'application
// -------------------------
app.Run();


public partial class Program { }
