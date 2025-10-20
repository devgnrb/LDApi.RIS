using LDApi.RIS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LDApi.RIS.Dto;

namespace LDApi.RIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HL7Controller : ControllerBase
    {

        private readonly HL7Service _hl7Service;

        public HL7Controller(HL7Service hl7Service)
        {
            _hl7Service = hl7Service;
        }

        [HttpPost("send")]
        public IActionResult SendHl7Message([FromBody] ReportDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request");

            try
            {
                var message = _hl7Service.GenerateHL7Message(dto,"clientApp","client");
                // Script pour envoie vers un serveur HL7 distant.
                return Ok(new { hl7 = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la génération du message HL7 : {ex.Message}");
            }
        }
    }
}
