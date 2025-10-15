using LDApi.RIS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LDApi.RIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _reportService.GetAllReports();
            return Ok(reports);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var fileBytes = _reportService.GetReportById(id);
            if (fileBytes == null)
                return NotFound();

            return File(fileBytes, "application/pdf");
        }
    }

}
