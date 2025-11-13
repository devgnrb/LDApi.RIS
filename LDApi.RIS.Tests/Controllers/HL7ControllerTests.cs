using LDApi.RIS.Controllers;
using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.ComponentModel.Design;
using System.Text.Json;

namespace LDApi.RIS.Tests.Controllers
{
    public class Hl7ControllerTests
    {
        private readonly Mock<IReportService> _mockReportService;
        private readonly Mock<IHL7Service> _mockHl7Service;
        private readonly Mock<IMllpClientService> _mockMllp;
        private readonly HL7Controller _controller;

        public Hl7ControllerTests()
        {
            _mockReportService = new Mock<IReportService>();
            _mockHl7Service = new Mock<IHL7Service>();
            _mockMllp = new Mock<IMllpClientService>();

            _controller = new HL7Controller(
                _mockHl7Service.Object,
                _mockMllp.Object,
                _mockReportService.Object
            );
        }

        [Fact]
        public async Task SendHl7MessageById_ShouldReturnOk_WhenValid()
        {
            var mockReportService = new Mock<IReportService>();
            mockReportService.Setup(r => r.GetAllReports()).ReturnsAsync(new List<ReportDto>
            {
                new ReportDto { IdReport = 1,             
                        FirstName = "John",
                        LastName = "Doe",
                        DateOfBirth = "19800101",
                        TypeDocument = "Laximétrie Dynamique",
                        DateReport = "20251107143000",
                        Path = "fakepath",
                        EnvoiHL7 = "" }
            });

            var mockHl7 = new Mock<IHL7Service>();
            mockHl7.Setup(h => h.GenerateHL7Message(It.IsAny<ReportDto>(), It.IsAny<string>(), It.IsAny<string>()))
                   .Returns("MSH|^~\\&|App|Client|Dest|Recv|20251113134902||RPA^R33|1234|P|2.3\rPID|1||1||DOE^JOHN||19800101");

            var mockMllp = new Mock<IMllpClientService>();
            mockMllp.Setup(m => m.SendMessageAsync(It.IsAny<string>()))
                    .ReturnsAsync("MSH|^~\\&|LDApiRIS|Genourob|LDApiRIS|Genourob|20251113134902||ACK^R33|1234|P|2.3\rMSA|AA|1234");

            var controller = new HL7Controller(mockHl7.Object, mockMllp.Object, mockReportService.Object);

            var dto = new HL7SendDto { Id = 1, Client = "Client", ClientApp = "App" };

            // Act
            var result = await controller.SendHl7MessageById(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            Console.WriteLine(json);
            var data = JsonSerializer.Deserialize<HL7ResponseDto>(json);

            try
            {
                if (data.Hl7 is null)
                {
                    throw new Exception("HL7 message is null");
                }
                else
                {
                    Assert.Contains("correcte", data.Hl7);
                }
                if (data.Ack is null)
                {
                    throw new Exception("ACK message is null");
                }
                else
                {

                    Assert.Contains("Accepté", data.Ack);
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message+"\n"+ex.StackTrace+"\n"+ex.Source);
            }


        }


        [Fact]
        public async Task SendHl7MessageById_ShouldReturnNotFound_WhenReportDoesNotExist()
        {
            // Arrange
            _mockReportService
                .Setup(s => s.GetAllReports())
                .ReturnsAsync(new List<ReportDto>());

            // Act
            var result = await _controller.SendHl7MessageById(new HL7SendDto { Client = "test", ClientApp = "testapp", Id = 99 });

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Rapport introuvable", notFoundResult.Value);
        }

        [Fact]
        public async Task SendHl7MessageById_ShouldReturn500_WhenMllpThrows()
        {
            // Arrange
            int reportId = 1;
            var fakeReports = new List<ReportDto>
            {
                new ReportDto { IdReport = reportId, LastName = "Doe", FirstName = "John", DateOfBirth = "19800101", DateReport = "20250101120012", Path = "/path/Doe.pdf", TypeDocument = "Laximétrie Dynamique", EnvoiHL7 = "Envoi Réussi" }
            };

            _mockReportService.Setup(s => s.GetAllReports()).ReturnsAsync(fakeReports);
            _mockHl7Service.Setup(s => s.GenerateHL7Message(It.IsAny<ReportDto>(), "clientApp", "client"))
                  .Returns("MSH|^~\\&|App|Client|clientApp|Recv|20251113134902||RPA^R33|123456|P|2.3\rPID|1||1||DOE^JOHN||19800101");
            _mockMllp.Setup(s => s.SendMessageAsync(It.IsAny<string>())).ThrowsAsync(new System.Exception("Connection error"));

            // Act
            var result = await _controller.SendHl7MessageById(new HL7SendDto { Client = "client", ClientApp = "clientApp", Id = reportId });

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Contains("Erreur HL7", statusResult.Value.ToString());
        }
    }
}
