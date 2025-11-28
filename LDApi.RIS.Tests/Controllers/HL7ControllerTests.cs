using LDApi.RIS.Controllers;
using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
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
        private readonly Mock<IReportYamlService> _mockYamlService;
        private readonly HL7Controller _controller;

        public Hl7ControllerTests()
        {
            _mockReportService = new Mock<IReportService>();
            _mockHl7Service = new Mock<IHL7Service>();
            _mockMllp = new Mock<IMllpClientService>();
            _mockYamlService = new Mock<IReportYamlService>();

            _controller = new HL7Controller(
                _mockHl7Service.Object,
                _mockMllp.Object,
                _mockReportService.Object,
                _mockYamlService.Object
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
                        EnvoiHL7 = StatusAck.AA }
            });

            var mockHl7 = new Mock<IHL7Service>();
            mockHl7.Setup(h => h.GenerateHL7Message(It.IsAny<ReportDto>(), It.IsAny<string>(), It.IsAny<string>()))
                   .Returns("MSH|^~\\&|App|Client|Dest|Recv|20251113134902||RPA^R33|1234|P|2.3\rPID|1||1||DOE^JOHN||19800101");

            var mockMllp = new Mock<IMllpClientService>();
            mockMllp.Setup(m => m.SendMessageAsync(It.IsAny<string>()))
                    .ReturnsAsync("MSH|^~\\&|LDApiRIS|Genourob|LDApiRIS|Genourob|20251113134902||ACK^R33|1234|P|2.3\rMSA|AA|1234");

            var mockYamlService = new Mock<IReportYamlService>();
            mockYamlService.Setup(s => s.SaveStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                StatusAck.AA
            )).Verifiable();

            var controller = new HL7Controller(mockHl7.Object, mockMllp.Object, mockReportService.Object,mockYamlService.Object);

            var dto = new HL7SendDto { Id = 1, Client = "Client", ClientApp = "App" };

            // Act
            var result = await controller.SendHl7MessageById(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var data = JsonSerializer.Deserialize<HL7ResponseDto>(json);

            Assert.Equal(StatusAck.AA,data.Ack);
            mockYamlService.Verify(
                s => s.SaveStatus(It.IsAny<int>(), It.IsAny<string>(), StatusAck.AA),
                Times.Once
            );
        }


        [Fact]
        public async Task SendHl7MessageById_ShouldReturnNOk_WhenNoValid()
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
                        EnvoiHL7 = StatusAck.AE }
            });

            var mockHl7 = new Mock<IHL7Service>();
            mockHl7.Setup(h => h.GenerateHL7Message(It.IsAny<ReportDto>(), It.IsAny<string>(), It.IsAny<string>()))
                   .Returns("MSH|^~\\&|App|Client|Dest|Recv|20251113134902||RPA^R33|1234|P|2.3\rPID|1||1||DOE^JOHN||19800101");

            var mockMllp = new Mock<IMllpClientService>();
            mockMllp.Setup(m => m.SendMessageAsync(It.IsAny<string>()))
                    .ReturnsAsync("MSH|^~\\&|LDApiRIS|Genourob|LDApiRIS|Genourob|20251113134902||ACK^R33|1234|P|2.3\rMSA|AE|1234");
                                var mockYamlService = new Mock<IReportYamlService>();
            mockYamlService.Setup(s => s.SaveStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                StatusAck.AE
            )).Verifiable();

            var controller = new HL7Controller(mockHl7.Object, mockMllp.Object, mockReportService.Object, mockYamlService.Object);

            var dto = new HL7SendDto { Id = 1, Client = "Client", ClientApp = "App" };

            // Act
            var result = await controller.SendHl7MessageById(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var data = JsonSerializer.Deserialize<HL7ResponseDto>(json);

            Assert.Equal(StatusAck.AE,data.Ack);
            mockYamlService.Verify(
                s => s.SaveStatus(It.IsAny<int>(), It.IsAny<string>(), StatusAck.AE),
                Times.Once
            );


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
                new ReportDto { IdReport = reportId, LastName = "Doe", FirstName = "John", DateOfBirth = "19800101", DateReport = "20250101120012", Path = "/path/Doe.pdf", TypeDocument = "Laximétrie Dynamique", EnvoiHL7 = StatusAck.AA }
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
                [Fact]
        public async Task SendBatch_ShouldReturnListOfAcks_AndSaveStatusForEach()
        {
            // Arrange
            var reports = new List<ReportDto>
            {
                new ReportDto { IdReport = 1, 
                FirstName = "John", 
                LastName = "Doe", Path = "path1", 
                DateOfBirth="01012000",
                DateReport="01012025112003",
                EnvoiHL7=StatusAck.NL,
                TypeDocument="pdf" },
                
                new ReportDto { IdReport = 2, 
                FirstName = "Jane", 
                LastName = "Smith", 
                Path = "path2", 
                DateOfBirth="11051987",
                DateReport="01112024112003",
                EnvoiHL7=StatusAck.AA,
                TypeDocument="pdf"}
            };

            _mockReportService.Setup(s => s.GetAllReports()).ReturnsAsync(reports);

            _mockHl7Service.Setup(h =>
                h.GenerateHL7Message(It.IsAny<ReportDto>(), It.IsAny<string>(), It.IsAny<string>())
            ).Returns("MSH...");

            _mockMllp.SetupSequence(m => m.SendMessageAsync(It.IsAny<string>()))
                .ReturnsAsync("MSH...\rMSA|AA|1234")
                .ReturnsAsync("MSH...\rMSA|AE|1234");

            _mockYamlService.Setup(s =>
                s.SaveStatus(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<StatusAck>())
            ).Verifiable();

            var controller = new HL7Controller(_mockHl7Service.Object, _mockMllp.Object,
                _mockReportService.Object, _mockYamlService.Object);

            var dto = new HL7SendDto { Client = "Client", ClientApp = "App" };

            // Act
            var result = await controller.SendBatch(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var payload = JsonSerializer.Deserialize<List<HL7ResponseDto>>(json);

            Assert.NotNull(payload);
            Assert.Equal(2, payload.Count);

            // Check ack mapping correctness
            Assert.Equal(StatusAck.AA, payload[0].Ack);
            Assert.Equal(StatusAck.AE, payload[1].Ack);

            // Check SaveStatus called twice with correct values
            _mockYamlService.Verify(
                s => s.SaveStatus(1, It.IsAny<string>(), StatusAck.AA),
                Times.Once);

            _mockYamlService.Verify(
                s => s.SaveStatus(2, It.IsAny<string>(), StatusAck.AE),
                Times.Once);
        }
        [Fact]
        public async Task SendBatch_ShouldContinue_WhenOneMessageFails()
        {
                    var reports = new List<ReportDto>
                    {
                        new ReportDto { IdReport = 1, 
                        FirstName = "John", 
                        LastName = "Doe", Path = "path1", 
                        DateOfBirth="01012000",
                        DateReport="01012025112003",
                        EnvoiHL7=StatusAck.NL,
                        TypeDocument="pdf" },
                        
                        new ReportDto { IdReport = 2, 
                        FirstName = "Jane", 
                        LastName = "Smith", 
                        Path = "path2", 
                        DateOfBirth="11051987",
                        DateReport="01112024112003",
                        EnvoiHL7=StatusAck.AA,
                        TypeDocument="pdf"}
                    };
            _mockReportService.Setup(s => s.GetAllReports()).ReturnsAsync(reports);

            _mockHl7Service.Setup(h => h.GenerateHL7Message(It.IsAny<ReportDto>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Returns("MSH...");

            _mockMllp.SetupSequence(m => m.SendMessageAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Connection error"))
                    .ReturnsAsync("MSH...\rMSA|AA|1234");

            var controller = new HL7Controller(_mockHl7Service.Object, _mockMllp.Object,
                _mockReportService.Object, _mockYamlService.Object);

            var dto = new HL7SendDto { Client = "Client", ClientApp = "App" };

            // Act
            var result = await controller.SendBatch(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var payload = JsonSerializer.Deserialize<List<HL7ResponseDto>>(json);

            Assert.NotNull(payload);
            Assert.Equal(2, payload.Count);
        }

    }
}
