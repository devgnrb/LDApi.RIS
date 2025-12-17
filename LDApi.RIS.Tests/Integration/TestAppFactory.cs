using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LDApi.RIS.Tests.Integration
{
    public class TestAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1) On retire les registrations réelles
                var descriptorReport = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IReportService));
                if (descriptorReport is not null)
                    services.Remove(descriptorReport);

                var descriptorMllp = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMllpClientService));
                if (descriptorMllp is not null)
                    services.Remove(descriptorMllp);

                // 2) On ajoute des mocks/fakes
                services.AddSingleton<IReportService, FakeReportService>();
                services.AddSingleton<IMllpClientService, FakeMllpClientService>();
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
}
