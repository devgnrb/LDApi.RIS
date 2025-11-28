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

        private readonly IReportService _reportService;
         private readonly IReportYamlService _yamlService;
        private readonly IMllpClientService _mllpClient;

        public HL7Controller(IHL7Service hl7Service,  IMllpClientService mllp, IReportService reportService, IReportYamlService yamlService)
        {
            _yamlService = yamlService;
            _hl7Service = hl7Service;
            _mllpClient = mllp;
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


            try
            {
                var ack = await _mllpClient.SendMessageAsync(message);
                  
       
                var segmentsAck = ack.Split('\r', '\n')
                                     .Where(s => !string.IsNullOrWhiteSpace(s))
                                     .ToArray();
                string ackCode = FileHelper.GetField(segmentsAck, "MSA", 1);
                StatusAck ackResponse = StatusAck.AR;
                
                switch(ackCode)
                {
                    case "AA":
                        ackResponse = StatusAck.AA;
                        break;
                    case "AE":
                        ackResponse = StatusAck.AE;
                        break;
                    case "AR":
                        ackResponse = StatusAck.AR;
                        break;
                }


                _yamlService.SaveStatus(report.IdReport, report.Path, ackResponse);
                

                return Ok(new { 

                    ack = ackResponse


                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Erreur HL7 : {ex.Message}");
            }
        }

    [HttpPost("send-batch")]
    public async Task<IActionResult> SendBatch([FromBody] HL7SendDto request)
    {

        var reports = await _reportService.GetAllReports();

        List<object> results = new();

        foreach (var report in reports)
        {
            try
            {
                var message = _hl7Service.GenerateHL7Message(report, request.ClientApp, request.Client);

                var ack = await _mllpClient.SendMessageAsync(message);

                var ackCode = FileHelper.GetField(
                    ack.Split('\r', '\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray(),
                    "MSA", 1);

                StatusAck status = ackCode switch
                {
                    "AA" => StatusAck.AA,
                    "AE" => StatusAck.AE,
                    "AR" => StatusAck.AR,
                    _ => StatusAck.NL
                };

                _yamlService.SaveStatus(report.IdReport, report.Path, status);

                results.Add(new { idReport = report.IdReport, ack = status });
            }
            catch (Exception ex)
            {
                results.Add(new { idReport = report.IdReport, error = ex.Message });
            }
        }

        return Ok(results);
    }



    }
}
