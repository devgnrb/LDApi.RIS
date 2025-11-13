using LDApi.RIS.Controllers;
using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LDApi.RIS.Tests.Controllers
{
    public class ReportsControllerTests
    {
        private readonly Mock<IReportService> _mockReportService;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _mockReportService = new Mock<IReportService>();
            _controller = new ReportsController(_mockReportService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithReports()
        {
            // Arrange
            // Arrange
            var fakeReports = new List<ReportDto>
            {
                new ReportDto { IdReport = 1, LastName = "Doe", FirstName = "John", DateOfBirth = "19800101", DateReport = "20250101120012", Path = "/path/Doe_John_19800101_20250101120012_20251023.pdf", TypeDocument = "Laximétrie Dynamique",EnvoiHL7="" },
                new ReportDto { IdReport = 2, LastName = "Smith", FirstName = "Jane", DateOfBirth = "19850202", DateReport = "20250102140523", Path = "/path/Smith_Jane_19850202_20250102140523_20251023.pdf", TypeDocument = "Laximétrie Dynamique",EnvoiHL7 = "Envoi Réussi" }
            };

            _mockReportService
                .Setup(s => s.GetAllReports())
                .ReturnsAsync(fakeReports);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ReportDto>>(okResult.Value);
            Assert.Equal(2, ((List<ReportDto>)model).Count);
        }

        [Fact]
        public void Get_ShouldReturnFile_WhenReportExists()
        {
            // Arrange
            int reportId = 1;
            var fakeBytes = new byte[] { 0x25, 0x50 }; // "%P" (juste un exemple de PDF)
            _mockReportService
                .Setup(s => s.GetReportById(reportId))
                .Returns(fakeBytes);

            // Act
            var result = _controller.Get(reportId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal(fakeBytes, fileResult.FileContents);
        }

        [Fact]
        public void Get_ShouldReturnNotFound_WhenReportDoesNotExist()
        {
            // Arrange
            int reportId = 99;
            _mockReportService
                .Setup(s => s.GetReportById(reportId))
                .Returns((byte[]?)null);

            // Act
            var result = _controller.Get(reportId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
