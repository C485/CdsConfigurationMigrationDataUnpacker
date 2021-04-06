using CdsConfigurationMigrationDataUnpacker.Extension;
using Dawn;
using System.IO;
using System.Text.Json;

namespace CdsConfigurationMigrationDataUnpacker.Helper
{
    public static class ConfigHelper
    {
        public static void SaveConfig<T>(string configFilePath, T config) where T : new()
        {
            string json = JsonSerializer.Serialize(config);
            File.WriteAllText(configFilePath, json);
        }

        public static T GetConfig<T>(string configFilePath) where T : new()
        {
            string configPath = Guard
                .Argument(configFilePath, nameof(configFilePath))
                .FileExists()
                .Value;
            string jsonData = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}