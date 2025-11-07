using LDApi.RIS.Dto;

namespace LDApi.RIS.Interfaces
{
    public interface IHL7Service
    {
        public string GenerateHL7Message(ReportDto dto, string targetApp, string targetFacility);
    }
}
