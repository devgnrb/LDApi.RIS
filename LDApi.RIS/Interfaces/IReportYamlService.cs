using LDApi.RIS.Dto;

namespace LDApi.RIS.Interfaces
{
    public interface IReportYamlService
    {
        void SaveStatus(int reportId, string pdfPath, StatusAck status);
        StatusAck LoadStatus(int reportId);
    }
}

