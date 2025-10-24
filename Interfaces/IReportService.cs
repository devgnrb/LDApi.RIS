using LDApi.RIS.Dto;

namespace LDApi.RIS.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportDto>> GetAllReports();
        byte[]? GetReportById(int id);
    }
}  
