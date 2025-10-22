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

        private readonly HL7Service _hl7Service;
        private readonly IMllpClientService _mllp;
        private readonly ReportService _reportService;
       public HL7Controller(HL7Service hl7Service, IMllpClientService mllp, ReportService reportService)
        {
            _hl7Service = hl7Service;
            _mllp = mllp;
            _reportService = reportService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendHl7Message([FromBody] ReportDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request");

            try
            {
                var message = _hl7Service.GenerateHL7Message(dto, "clientApp", "client");
                var ack = await _mllp.SendMessageAsync(message);

                return Ok(new { hl7 = message, ack });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la génération du message HL7 : {ex.Message}");
            }
        }

        [HttpPost("send-batch")]
        public async Task<IActionResult> SendHl7Messages()
        {
            var dtos = await _reportService.GetAllReports();
            if (dtos == null || !dtos.Any())
                return BadRequest("La liste de rapports est vide ou invalide");

            var results = new List<object>();

            foreach (var dto in dtos)
            {
                try
                {
                    // Génération du message HL7
                    var message = _hl7Service.GenerateHL7Message(dto, "clientApp", "client");

                    // Envoi via MLLP
                    var ack = await _mllp.SendMessageAsync(message);

                    results.Add(new { hl7 = message, ack, reportLastName = dto.LastName });
                }
                catch (Exception ex)
                {
                    results.Add(new { reportLastName = dto.LastName, error = ex.Message });
                }
            }

            return Ok(results);
        }

        [HttpPost("send/{id}")]
        public async Task<IActionResult> SendHl7MessageById(int id)
        {
            var reports = await _reportService.GetAllReports();
            var report = reports.FirstOrDefault(r => r.IdReport == id);

            if (report == null)
                return NotFound("Rapport introuvable");
            var message = _hl7Service.GenerateHL7Message(report, "clientApp", "client");
          
            try
            {
                
                var ack = await _mllp.SendMessageAsync(message);

                return Ok(new { hl7 = message, ack });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur HL7 : {ex.Message} ");
            }
        }


    }
}
