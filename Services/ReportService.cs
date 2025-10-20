using LDApi.RIS.Dto;
using LDApi.RIS.Utils;

namespace LDApi.RIS.Services
{
    public class ReportService
    {
        private readonly string _reportDirectory;

        public ReportService(IConfiguration config)
        {
            _reportDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Pdf_Dyneelax"
);
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
                var parts = fileName.Split('_');
                ReportDto r = new ReportDto()
                {
                    IdReport = cpt++,
                    LastName = parts.Length > 0 ? parts[0] : "Unknown",
                    FirstName = parts.Length > 1 ? parts[1] : "Unknown",
                    DateOfBirth = "Unknown",
                    DateReport = parts.Length > 2 ? parts[2] : "Unknown",
                    Path = pathFileName + fileName + ".pdf",
                    TypeDocument = "Laximétrie Dynamique"
                };
                reports.Add(r);



            }
            return reports;

        }

        public byte[]? GetReportById(int id)
        {
            var files = FileHelper.GetPdfFiles(_reportDirectory);
            var file = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).GetHashCode() == id);

            return file != null ? File.ReadAllBytes(file) : null;
        }
    }
}
