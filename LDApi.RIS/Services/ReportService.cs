using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using LDApi.RIS.Utils;
using PdfSharp.Pdf.IO;
using Xunit;

namespace LDApi.RIS.Services
{
    public class ReportService : IReportService
    {
        private readonly string _reportDirectory;

        // generate a singleton service 

        public ReportService(IConfiguration config)
        {
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
                var metaService = new MetaDataPDFService();
                ReportDto r = new ReportDto()
                {
                    IdReport = cpt++,
                    LastName = metadata.Length > 0 ? metadata[0] : "Unknown",
                    FirstName = metadata.Length > 1 ? metadata[1] : "Unknown",
                    DateOfBirth = metadata.Length > 2 ? metadata[2] : "Unknown",
                    DateReport = metadata.Length > 3 ? metadata[3] : "Unknown",
                    Path = pathFileName + "\\" + fileName + ".pdf",
                    TypeDocument = "Laximétrie Dynamique",
                    EnvoiHL7 = metaService.GetMetaDataValue(pathFileName + "\\" + fileName + ".pdf", "EnvoiHL7") ?? ""
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
}
