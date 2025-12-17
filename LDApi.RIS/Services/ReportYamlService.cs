using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;

namespace LDApi.RIS.Services
{
    public class ReportYamlService : IReportYamlService
    {
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private readonly IHostEnvironment _env;

        //  On injecte IHostEnvironment ici
        public ReportYamlService(IHostEnvironment env)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));

            _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        // Crée ou récupère le dossier ReportStatus à la racine de l'API
        private string GetStatusDirectory()
        {
            // Exemple : bin/Debug/net8.0/ReportStatus ou publish/ReportStatus
            string statusDir = Path.Combine(_env.ContentRootPath, "ReportStatus");

            if (!Directory.Exists(statusDir))
                Directory.CreateDirectory(statusDir);

            return statusDir;
        }

        public void SaveStatus(int reportId, string pdfPath, StatusAck status)
        {

            var reportData = new
            {
                ReportId = reportId,
                FileName = Path.GetFileName(pdfPath),
                EnvoiHL7 = status,
                Date = DateTime.UtcNow
            };
      
            string yaml = _serializer.Serialize(reportData);
            string filePath = Path.Combine(GetStatusDirectory(), $"Report_{reportId}.yml");

            File.WriteAllText(filePath, yaml);
        }

        // Option utile : charger le statut à partir du YAML
        public StatusAck LoadStatus(int reportId)
        {
            try
            {
                string filePath = Path.Combine(GetStatusDirectory(), $"Report_{reportId}.yml");
                if (!File.Exists(filePath))
                    return StatusAck.NL;

                string yaml = File.ReadAllText(filePath);
                var data = _deserializer.Deserialize<ReportYamlStatus>(yaml);

                return Enum.TryParse<StatusAck>(data.EnvoiHL7?.ToString(), true, out var parsed)
                    ? parsed
                    : StatusAck.NL;
            }
            catch
            {
                return StatusAck.NL;
            }
        }

        // DTO pour désérialisation YAML (optionnel, si tu en as besoin)
        private class ReportYamlStatus
        {
            public int ReportId { get; set; }
            public string? FileName { get; set; }
            public StatusAck? EnvoiHL7 { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
