    using LDApi.RIS.Dto;
    using LDApi.RIS.Interfaces;
    using LDApi.RIS.Utils;
    using PdfSharp.Pdf.IO;
    using Xunit;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using System.IO;

namespace LDApi.RIS.Services
{
    public class ReportService : IReportService
    {
        private readonly string _reportDirectory;
        private readonly ReportYamlService _yamlService;
        // generate a singleton service 

        public ReportService(IConfiguration config, ReportYamlService yamlService)
        {
            _yamlService = yamlService;
            _reportDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Pdf_Dyneelax");
        }

        public async Task<IEnumerable<ReportDto>> GetAllReports()
        {
            var files = FileHelper.GetPdfFiles(_reportDirectory);

            List<ReportDto> reports = new List<ReportDto>();
            int cpt = 0;
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string? pathFileName = Path.GetDirectoryName(file);
                // gestion du pdf nommé prénom dateNaissance dateReport
                PdfSharp.Pdf.PdfDocument document = PdfSharp.Pdf.IO.PdfReader.Open(file, PdfDocumentOpenMode.Import);

                var metadata = document.Info.Keywords.Split(",");
                var status = _yamlService.LoadStatus(cpt);

                ReportDto r = new ReportDto()
                {
                    IdReport = cpt,
                    LastName = metadata.Length > 0 ? metadata[0] : "Unknown",
                    FirstName = metadata.Length > 1 ? metadata[1] : "Unknown",
                    DateOfBirth = metadata.Length > 2 ? metadata[2] : "Unknown",
                    DateReport = metadata.Length > 3 ? metadata[3] : "Unknown",
                    Path = pathFileName + "\\" + fileName + ".pdf",
                    TypeDocument = "Laximétrie Dynamique",
                    EnvoiHL7 = status
                };
                reports.Add(r);
                cpt++;
            }
            return reports;
        }

    private StatusAck GetReportStatusByYml(int id, string pathDirectory)
    {
        try
        {
            var statusDirectory = Path.Combine(pathDirectory ?? "", "ReportStatus");
            var yamlFile = Path.Combine(statusDirectory, $"Report_{id}.yml");

            if (File.Exists(yamlFile))
            {
                var yaml = File.ReadAllText(yamlFile);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var yamlStatus = deserializer.Deserialize<ReportYamlStatus>(yaml);

                return Enum.TryParse<StatusAck>(yamlStatus.EnvoiHL7?.ToString(), true, out var status)
                    ? status
                    : StatusAck.NL;
            }

            return StatusAck.NL;
        }
        catch
        {
            return StatusAck.NL;
        }
    }



        public byte[]? GetReportById(int id)
        {

            var files = FileHelper.GetPdfFiles(_reportDirectory).ToList();

            if (id < 0 || id >= files.Count)
                return null;

            var file = files[id];
            return File.ReadAllBytes(file);
        }

        public ReportDto GetReport(int id)
        {

            var r = GetAllReports().Result.FirstOrDefault(r => r.IdReport == id);
            if (r == null)
            {
                throw new Exception("Report not found");
            }
            return r;
        }

    }

    // Mapping YAML
    public class ReportYamlStatus
    {
        public int ReportId { get; set; }
        public string? FileName { get; set; }
        public StatusAck? EnvoiHL7 { get; set; }
        public DateTime Date { get; set; }
    }
}
