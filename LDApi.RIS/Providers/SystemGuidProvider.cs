using LDApi.RIS.Interfaces;

namespace LDApi.RIS.Providers
{
    public class SystemGuidProvider : IGuidProvider
    {
        public Guid NewGuid() => Guid.NewGuid();
    }
}
