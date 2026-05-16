using System.Text.Json;
using System.Text.Json.Serialization;

namespace RpaScHauStory
{
    public class AppConfig
    {
        public string CdpEndpoint { get; set; } = "http://127.0.0.1:9222";
        public List<TabConfig> Tabs { get; set; } = [];

        private static readonly string ConfigPath =
            Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,   // JSON 파일에 주석 허용
        };

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigPath))
                return new AppConfig();

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }

        public static void Save(AppConfig config)
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
    }

    public class TabConfig
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }
}
