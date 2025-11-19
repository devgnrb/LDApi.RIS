using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
using LDApi.RIS.Providers;
using System.ComponentModel;
using Microsoft.Extensions.FileProviders;

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

// Active les contr�leurs API
builder.Services.AddControllers();

// -------------------------
// CORS pour ton front React
// -------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("*")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// -------------------------
// Middleware
// -------------------------
//app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowReact");



// Servir les fichiers statiques
var uiPath = Path.Combine(Directory.GetCurrentDirectory(),"../ldapi-ris-ts/build");
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(uiPath)
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uiPath),
    RequestPath = ""
});

app.MapFallbackToFile("index.html",new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uiPath)
});




// Mappe les contr�leurs
app.MapControllers();
// -------------------------
// Lancement de l'application
// -------------------------
app.Run("http://0.0.0.0:5033");


public partial class Program { }
