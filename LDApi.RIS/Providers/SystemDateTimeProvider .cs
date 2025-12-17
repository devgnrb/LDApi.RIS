using LDApi.RIS.Interfaces;

namespace LDApi.RIS.Providers
{
    public class SystemDateTimeProvider: IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
