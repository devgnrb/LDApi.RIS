namespace LDApi.RIS.Interfaces
{
    public interface IMllpConfigurationService
    {
        string Host { get; set; }
        int Port
        {
            get; set;
        }
    }
}
