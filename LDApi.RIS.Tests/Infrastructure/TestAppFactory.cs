using LDApi.RIS.Data;
using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace LDApi.RIS.Tests.Infrastructure
{
    internal class TestAppFactory : WebApplicationFactory<Program>
    {
        private readonly TestMode _mode;

        public TestAppFactory(TestMode mode)
        {
            _mode = mode;
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.ConfigureTestServices(services =>
            {
                // --- Mode AuthOnly : désactiver services métier ---
                if (_mode == TestMode.AuthOnly)
                {
                    services.RemoveAll(typeof(IReportService));
                    services.RemoveAll(typeof(IReportYamlService));
                    services.RemoveAll(typeof(IMllpClientService));

                    // --- Appliquer les migrations pour que AspNetUsers existe ---
                    using (var sp = services.BuildServiceProvider())
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                        db.Database.Migrate(); // <-- essentiel
                        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                        var tables = context.Database.ExecuteSqlRaw(
                            "SELECT name FROM sqlite_master WHERE type='table';");
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
                    services.AddScoped(_ =>
                    {
                        var mockReport = new Mock<IReportService>();
                        // si nécessaire : mockReport.Setup(...)
                        return mockReport.Object;
                    });
                    services.AddScoped(_ =>
                    {
                        var mockReportYAML = new Mock<IReportYamlService>();
                        // si nécessaire : mockReportYAML.Setup(...)
                        return mockReportYAML.Object;
                    });
                    services.AddScoped(_ =>
                    {
                        var mockMllp = new Mock<IMllpClientService>();
                        // si nécessaire : mockMllp.Setup(...)
                        return mockMllp.Object;
                    });
                }


            });
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
