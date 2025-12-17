namespace LDApi.RIS.Config
{
    public class AppConfig
    {
        public RisConfig Ris { get; set; } = new();
        public MllpConfig Mllp { get; set; } = new();
    }

    public class RisConfig
    {
        public string Host { get; set; } = "http://localhost:5033";
        public string Client { get; set; } = "client";
        public string ClientApp { get; set; } = "clientApp";
    }

    public class MllpConfig
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 6661;
    }
}
