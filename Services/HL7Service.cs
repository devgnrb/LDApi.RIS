using LDApi.RIS.Dto;
using System.Text;

namespace LDApi.RIS.Services
{
    public class HL7Service
    {

        public string GenerateHL7Message(ReportDto dto, string clientApp, string client)
        {
            var sb = new StringBuilder();

            // Génération de l'identifiant unique du message
            string messageControlId = Guid.NewGuid().ToString("N").Substring(0, 10);

            // Construction des segments HL7
            // Segment MSH
            sb.AppendLine($"MSH|^~\\&|LDApiRIS|Genourob|{TargetApp}|{TargetFacility}|{DateTime.Now:yyyyMMddHHmmss}||ORU^R01|{messageControlId}|P|2.3");

            // PID - Patient Identification
            // Conversion de la date de naissance en format YYYYMMDD
            DateTime date = DateTime.ParseExact(dto.DateOfBirth, "ddMMyyyy", null);
            string formattedDateOfBirth = date.ToString("yyyyMMdd");

            sb.AppendLine($"PID|1||{dto.IdReport}||{dto.LastName}^{dto.FirstName}||{formattedDateOfBirth:yyyyMMdd}|||||||||||");

            // TXA - Document notification


            sb.AppendLine($"TXA|1|PN|{dto.TypeDocument}|{dto.TypeDocument}|{dto.DateReport:yyyyMMddHHmmss}|||{dto.Path}||||||||||");

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
