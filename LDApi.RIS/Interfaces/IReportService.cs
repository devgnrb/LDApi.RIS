using LDApi.RIS.Dto;
using Xunit;

namespace LDApi.RIS.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportDto>> GetAllReports();
        byte[]? GetReportById(int id);
        ReportDto? GetReport(int id);

    }
}
