using LDApi.RIS.Data;
using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;

namespace LDApi.RIS.Tests.Infrastructure
{
    internal class TestAppFactory : WebApplicationFactory<Program>
    {
        private readonly TestMode _mode;
        private SqliteConnection? _connection;

        public TestAppFactory(TestMode mode) => _mode = mode;
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience",
                    ["Jwt:Secret"] = "THIS_IS_A_TEST_SECRET_KEY_123456789_123456789"
                });
            });
            builder.ConfigureTestServices(services =>
            {
                // --- Mode AuthOnly : désactiver services métier ---
                if (_mode == TestMode.AuthOnly)
                {
                    services.RemoveAll(typeof(IReportService));
                    services.RemoveAll(typeof(IReportYamlService));
                    services.RemoveAll(typeof(IMllpClientService));


                    // 1) Retirer le AuthDbContext existant 
                    services.RemoveAll(typeof(DbContextOptions<AuthDbContext>));

                    // 2) Créer une connexion SQLite in-memory unique pour cette factory
                    _connection = new SqliteConnection("DataSource=:memory:");
                    _connection.Open();

                    // 3) Ré-enregistrer AuthDbContext sur SQLite in-memory
                    services.RemoveAll(typeof(DbContextOptions<AuthDbContext>));

                    _connection = new SqliteConnection("DataSource=:memory:");
                    _connection.Open();

                    services.AddDbContext<AuthDbContext>(options =>
                        options.UseSqlite(_connection));

                    // 4) Appliquer les migrations + créer le rôle dans LE MÊME scope
                    using var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();

                    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                    db.Database.Migrate();

                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    foreach (var role in new[] { "User", "Admin" })
                    {
                        if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                        {
                            var result = roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                            if (!result.Succeeded)
                            {
                                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                                throw new InvalidOperationException($"Failed to create role 'User': {errors}");
                            }
                        }
                    }

                }
                if (_mode == TestMode.FullMockServices)
                {

                    // Supprimer les services réels
                    var descriptorReport = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IReportService));
                    if (descriptorReport != null)
                        services.Remove(descriptorReport);

                    var descriptorYamlReport = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IReportYamlService));
                    if (descriptorYamlReport != null)
                        services.Remove(descriptorYamlReport);

                    var descriptorMllp = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IMllpClientService));
                    if (descriptorMllp != null)
                        services.Remove(descriptorMllp);

                    // Ajoute des mocks/fakes
                    services.AddScoped<IReportService>(_ => new FakeReportService());
                    services.AddScoped<IReportYamlService>(_ => new FakeReportYamlService());
                    services.AddScoped<IMllpClientService>(_ => new FakeMllpClientService());
                    services.AddScoped(_ =>
                    {
                        var mockReport = new Mock<IReportService>();
                        // si nécessaire : mockReport.Setup(...)
                        return mockReport.Object;
                    });
                    services.AddScoped(_ =>
                    {
                        var mockReport = new Mock<IReportService>();
                        mockReport.Setup(s => s.GetAllReports())
                                .ReturnsAsync(new List<ReportDto>
                                {
                                    new ReportDto { IdReport = 1, FirstName="John",
                                    LastName="Doe", DateOfBirth="01/01/2001",DateReport="25/11/2025",
                                    Path="\\PdfDyneelax\toot.pdf",
                                    TypeDocument="Laximetrie Dynamique",
                                    EnvoiHL7 = StatusAck.NL  },
                                    new ReportDto { IdReport = 2, FirstName="Anna",
                                    LastName="Smith",  DateOfBirth="01/01/1991",DateReport="30/12/2024",
                                    Path="\\PdfDyneelax\anna_smith.pdf",
                                    TypeDocument="Laximetrie Dynamique",
                                    EnvoiHL7 = StatusAck.NL  },
                                });
                        return mockReport.Object;
                    });
                    services.AddScoped(_ =>
                    {
                        var mockMllp = new Mock<IMllpClientService>();
                        mockMllp.Setup(s => s.SendMessageAsync(It.Is<string>(m => m.Contains("Doe"))))
                                .ReturnsAsync("MSH|^~\\&|LDApiRIS|Genourob|LDApiRIS|Genourob|20251113134902||ACK^R33|1234|P|2.3\rMSA|AA|1");
                        mockMllp.Setup(s => s.SendMessageAsync(It.Is<string>(m => m.Contains("Smith"))))
                                .ReturnsAsync("MSH|^~\\&|LDApiRIS|Genourob|LDApiRIS|Genourob|20251113134902||ACK^R33|1234|P|2.3\rMSA|AE|2");
                        return mockMllp.Object;
                    });
                }


            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection?.Dispose();
                _connection = null;
            }
        }
    }

    // Fake simple pour IReportService
    public class FakeReportService : IReportService
    {
        public Task<IEnumerable<ReportDto>> GetAllReports()
        {
            var reports = new List<ReportDto>
            {
                new ReportDto
                {
                    IdReport = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = "19800101",
                    DateReport = "20250101123000",
                    Path = "fakepath1.pdf",
                    TypeDocument = "Laximétrie Dynamique",
                    EnvoiHL7 = StatusAck.NL
                },
                new ReportDto
                {
                    IdReport = 2,
                    FirstName = "Anna",
                    LastName = "Smith",
                    DateOfBirth = "19850912",
                    DateReport = "20250102120000",
                    Path = "fakepath2.pdf",
                    TypeDocument = "Laximétrie Dynamique",
                    EnvoiHL7 = StatusAck.NL
                }
            };

            return Task.FromResult<IEnumerable<ReportDto>>(reports);
        }

        public Task<byte[]?> GetReportById(int id) => Task.FromResult<byte[]?>(null);
        public ReportDto GetReport(int id) => throw new NotImplementedException();

        byte[]? IReportService.GetReportById(int id)
        {
            throw new NotImplementedException();
        }
    }

    // Fake MLLP qui renvoie un ACK AA pour le 1er, AE pour le 2e
    public class FakeMllpClientService : IMllpClientService
    {
        public Task<string> SendMessageAsync(string hl7Message)
        {
            // On simule deux messages avec ACK AA et ACK AE
            if (hl7Message.Contains("Doe"))
            {
                return Task.FromResult("MSH|^~\\&|...|...|...|...|20250101123000||ACK^R33|1|P|2.3\rMSA|AA|1");
            }

            return Task.FromResult("MSH|^~\\&|...|...|...|...|20250102120000||ACK^R33|2|P|2.3\rMSA|AE|2");
        }
    }

    // Fake YAML 
    public class FakeReportYamlService : IReportYamlService
    {
        private readonly Dictionary<int, StatusAck> _statusStore = new();

        public void SaveStatus(int reportId, string pdfPath, StatusAck status)
        {
            _statusStore[reportId] = status;
        }

        public StatusAck LoadStatus(int reportId)
        {
            if (_statusStore.TryGetValue(reportId, out var status))
                return status;

            return StatusAck.NL;  // valeur par défaut si non trouvé
        }



    }


}
