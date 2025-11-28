using LDApi.RIS.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LDApi.RIS.Services
{
    public class ConfigurationService
    {
        private readonly string _configFilePath;
        public AppConfig Config { get; private set; }

        public ConfigurationService(IHostEnvironment env)
        {
            // Chemin du fichier config.yml
            // => dans le répertoire de l'application (bin/Debug..., bin/Release..., ou publish/)
            _configFilePath = Path.Combine(env.ContentRootPath, "config.yml");

            Config = LoadConfig();
        }

        private AppConfig LoadConfig()
        {
            if (!File.Exists(_configFilePath))
            {
                // Fichier absent → on crée une config par défaut
                var defaultConfig = new AppConfig();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            var yaml = File.ReadAllText(_configFilePath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var cfg = deserializer.Deserialize<AppConfig>(yaml);
            return cfg ?? new AppConfig();
        }

        public void SaveConfig(AppConfig config)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(config);
            File.WriteAllText(_configFilePath, yaml);

            Config = config;
        }
    }
}
