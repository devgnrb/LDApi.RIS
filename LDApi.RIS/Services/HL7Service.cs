using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using System.Text;
namespace LDApi.RIS.Services
{
    public class HL7Service : IHL7Service
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IGuidProvider _guidProvider;

        public HL7Service(IDateTimeProvider dateTimeProvider, IGuidProvider guidProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _guidProvider = guidProvider;
        }
        public string GenerateHL7Message(ReportDto dto, string targetApp, string targetFacility)
        {
            var sb = new StringBuilder();

            // Génération de l'identifiant unique du message
            string messageControlId = _guidProvider.NewGuid().ToString("N").Substring(0, 10);

            // Construction des segments HL7
            // Segment MSH
            sb.AppendLine($"MSH|^~\\&|LDApiRIS|Genourob|{targetApp}|{targetFacility}|{DateTime.Now:yyyyMMddHHmmss}||RPA^R33|{messageControlId}|P|2.3");

            // PID - Patient Identification


            sb.AppendLine($"PID|1||{dto.IdReport}||{dto.LastName}^{dto.FirstName}||{dto.DateOfBirth:yyyyMMdd}|||||||||||");

            // TXA - Document notification


            sb.AppendLine($"TXA|1|PN|{dto.TypeDocument}|{dto.DateReport:yyyyMMddHHmmss}|||{dto.DateReport:yyyyMMdd}||||||||||");

            // OBX - Observation segment (base64 du PDF éventuellement)
            if (File.Exists(dto.Path))
            {
                var pdfBytes = File.ReadAllBytes(dto.Path);
                var base64 = Convert.ToBase64String(pdfBytes);

                sb.AppendLine($"OBX|1|ED|PDF^Base64^HL7|1|^{base64}||||||F");
            }

            // Implémentation de la génération du message HL7
            return sb.ToString();
        }
    }
}
