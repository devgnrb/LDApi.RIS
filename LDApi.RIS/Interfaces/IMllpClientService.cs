namespace LDApi.RIS.Interfaces
{
    public interface IMllpClientService
    {
        Task<string> SendMessageAsync(string hl7Message);
    }
}
