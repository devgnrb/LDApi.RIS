using LDApi.RIS.Interfaces;

namespace LDApi.RIS.Services
{
    public class MllpConfigurationService : IMllpConfigurationService
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 6661;
    }
}
