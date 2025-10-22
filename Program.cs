using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Ajout des services
// -------------------------
builder.Services.AddSingleton<ReportService>();

builder.Services.AddSingleton<IMllpClientService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var host = config["Mllp:Host"] ?? "127.0.0.1";
    var port = int.Parse(config["Mllp:Port"] ?? "6661");
    return new MllpClientService(host, port);
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

// -------------------------
// Lancement de l'application
// -------------------------
app.Run();
