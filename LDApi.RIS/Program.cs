using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
using LDApi.RIS.Providers;
using System.ComponentModel;
using Microsoft.Extensions.FileProviders;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// dotnet test (host environ test)
var isTesting = builder.Environment.EnvironmentName == "Development" &&
                AppDomain.CurrentDomain.FriendlyName.Contains("test", StringComparison.OrdinalIgnoreCase);
// -------------------------
// Ajout des services
// -------------------------
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IHL7Service, HL7Service>();
builder.Services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<IGuidProvider, SystemGuidProvider>();
builder.Services.AddScoped<IMllpClientService, MllpClientService>();
builder.Services.AddScoped<IReportYamlService, ReportYamlService>();

builder.Services.AddSingleton<ConfigurationService>();
builder.Services.AddScoped<HL7Service>();
builder.Services.AddScoped<ReportYamlService>();

// Active les controleurs API
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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

app.UseCors("AllowReact");


if (!isTesting)  // ⬅️ évite le code UI en tests
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

    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uiPath)
    });
}

app.MapControllers();
app.Run("http://0.0.0.0:5033");
// -------------------------
// Middleware
// -------------------------
//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.UseCors("AllowReact");



// Servir les fichiers statiques
//var uiPath = Path.Combine(Directory.GetCurrentDirectory(),"../ldapi-ris-ts/build");
/*var uiPath = Path.Combine(Directory.GetCurrentDirectory(),"../wwwroot");
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
app.Run("http://0.0.0.0:5033");*/


public partial class Program { }
