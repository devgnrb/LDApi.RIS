using LDApi.RIS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;

namespace LDApi.RIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HL7Controller : ControllerBase
    {

        private readonly IHL7Service _hl7Service;
        private readonly IMllpClientService _mllp;
        private readonly IReportService _reportService;

       public HL7Controller(IHL7Service hl7Service, IMllpClientService mllp, IReportService reportService)
        {
            _hl7Service = hl7Service;
            _mllp = mllp;
            _reportService = reportService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendHl7MessageById([FromBody] HL7SendDto request)
        {
            var reports = await _reportService.GetAllReports();
            var report = reports.FirstOrDefault(r => r.IdReport == request.Id);

            if (report == null)
                return NotFound("Rapport introuvable");

            // Utilise les valeurs envoyées par le front
            var message = _hl7Service.GenerateHL7Message(report, request.ClientApp, request.Client);

            try
            {
                var ack = await _mllp.SendMessageAsync(message);
                return Ok(new { hl7 = message, ack });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur HL7 : {ex.Message}");
            }
        }


    }
}
