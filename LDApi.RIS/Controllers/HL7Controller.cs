using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
using LDApi.RIS.Utils;
using Microsoft.AspNetCore.Mvc;
using Xunit;

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
            var segments = message.Split('\r', '\n')
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .ToArray();

            string messageHl7 = "";

            string msh10 = FileHelper.GetField(segments, "MSH", 9);

                // MSH-10 non vide et format valide
                if (String.IsNullOrEmpty(msh10))
                    messageHl7 = "Message Control ID modifié par le client : " + msh10 + "est incorrecte";
                else
                {
                    messageHl7 = "Message Control ID : " + msh10 + " est correcte";
                }
            

   

            try
            {
                var ack = await _mllp.SendMessageAsync(message);
                var segmentsAck = ack.Split('\r', '\n')
                                     .Where(s => !string.IsNullOrWhiteSpace(s))
                                     .ToArray();
                string ackCode = FileHelper.GetField(segmentsAck, "MSA", 1);
                string ackResponse = "";
                switch(ackCode)
                {
                    case "AA":
                        ackResponse = "Accusé de réception : Accepté";
                        break;
                    case "AE":
                        ackResponse = "Accusé de réception : Rejeté - Erreur";
                        break;
                    case "AR":
                        ackResponse = "Accusé de réception : Rejeté - Refusé";
                        break;
                    default:
                        ackResponse = "Accusé de réception : Code inconnu";
                        break;
                }
                // Inserer une meta donnée de l'envoi HL7 dans le rapport 
                if (ack != null)
                {
                    ReportYamlService reportYamlService = new ReportYamlService();
                    reportYamlService.SaveStatus(report.IdReport, report.Path, "Envoi Réussi");
                }

                return Ok(new { 

                    hl7 = messageHl7, 
                    
                    ack = ackResponse


                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur HL7 : {ex.Message}");
            }
        }

        [HttpGet("pdf/envoi-status/{id}")]
        public IActionResult GetEnvoiStatus(int id)
        {
            var report = _reportService.GetReport(id);
            if (report == null)
                return NotFound("Rapport introuvable");
;
            string path = Path.GetDirectoryName(report.Path) ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Pdf_Dyneelax");

            var statusService = new ReportYamlService(); 
            var status = statusService.GetStatus(id,path) ?? "Non envoyé";

            return Ok(new { envoiHL7 = status });
        }

    }
}
