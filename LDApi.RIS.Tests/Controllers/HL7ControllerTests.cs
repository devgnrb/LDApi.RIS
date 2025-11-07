using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using LDApi.RIS.Controllers;
using LDApi.RIS.Services;
using LDApi.RIS.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDApi.RIS.Interfaces;
using System.ComponentModel.Design;

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
        public async Task SendHl7MessageById_ShouldReturnOk_WhenReportExists()
        {
            // Arrange
            int reportId = 1;
            var fakeReports = new List<ReportDto>
            {
                new ReportDto { IdReport = reportId, LastName = "Doe", FirstName = "John", DateOfBirth = "19800101", DateReport = "20250101120012", Path = "/path/Doe.pdf", TypeDocument = "Laximétrie Dynamique", EnvoiHL7 ="" }
            };

            _mockReportService
                .Setup(s => s.GetAllReports())
                .ReturnsAsync(fakeReports);

            _mockHl7Service
                .Setup(s => s.GenerateHL7Message(It.IsAny<ReportDto>(), "clientApp", "client"))
                .Returns("FAKE_HL7_MESSAGE");

            _mockMllp
                .Setup(s => s.SendMessageAsync("FAKE_HL7_MESSAGE"))
                .ReturnsAsync("ACK");

            // Act
            var result = await _controller.SendHl7MessageById(new HL7SendDto { Client="test", ClientApp="testapp",Id = reportId});

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;
            Assert.Equal("FAKE_HL7_MESSAGE", value.hl7);
            Assert.Equal("ACK", value.ack);
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
            _mockHl7Service.Setup(s => s.GenerateHL7Message(It.IsAny<ReportDto>(), "clientApp", "client")).Returns("FAKE_HL7_MESSAGE");
            _mockMllp.Setup(s => s.SendMessageAsync(It.IsAny<string>())).ThrowsAsync(new System.Exception("Connection error"));

            // Act
            var result = await _controller.SendHl7MessageById(new HL7SendDto { Client = "test", ClientApp = "testapp", Id = reportId });

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Contains("Erreur HL7", statusResult.Value.ToString());
        }
    }
}
