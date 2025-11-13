namespace LDApi.RIS.Services
{
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using System.IO;

    public class ReportYamlService
    {
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        public ReportYamlService()
        {
            _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        // Crée ou récupère le dossier ReportStatus à côté du PDF
        private string GetStatusDirectory(string pdfPath)
        {
            string pdfDir = Path.GetDirectoryName(pdfPath)!;
            string statusDir = Path.Combine(pdfDir, "ReportStatus");

            if (!Directory.Exists(statusDir))
                Directory.CreateDirectory(statusDir);

            return statusDir;
        }

        public void SaveStatus(int reportId, string pdfPath, string status)
        {
            var reportData = new
            {
                ReportId = reportId,
                FileName = Path.GetFileName(pdfPath),
                EnvoiHL7 = status,
                Date = DateTime.UtcNow
            };

            string yaml = _serializer.Serialize(reportData);
            string filePath = Path.Combine(GetStatusDirectory(pdfPath), $"Report_{reportId}.yml");

            File.WriteAllText(filePath, yaml);
        }

        public string? GetStatus(int reportId, string pdfPath)
        {
            string filePath = Path.Combine(GetStatusDirectory(pdfPath), $"Report_{reportId}.yml");

            if (!File.Exists(filePath))
                return null;

            string yaml = File.ReadAllText(filePath);
            dynamic? data = _deserializer.Deserialize<dynamic>(yaml);
            return data?["EnvoiHL7"];
        }
    }



}
