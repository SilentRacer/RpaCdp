using System.Text.Json;

namespace RpaScHauStory
{
    /// <summary>앱 전체 설정을 로드·저장합니다.</summary>
    public class AppConfig
    {
        public string CdpEndpoint { get; set; } = "http://127.0.0.1:9222";
        public int Columns { get; set; } = 1;
        public List<TabConfig> Tabs { get; set; } = [];

        // C:\ProgramData\KTJ\CDP\appsettings.json 이 있으면 우선 사용, 없으면 앱 폴더의 기본값 사용
        private static readonly string UserConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KTJ", "CDP", "appsettings.json");

        private static readonly string DefaultConfigPath =
            Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public static AppConfig Load()
        {
            var path = File.Exists(UserConfigPath) ? UserConfigPath : DefaultConfigPath;
            if (!File.Exists(path)) return new AppConfig();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }

        public static void Save(AppConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(UserConfigPath)!);
            File.WriteAllText(UserConfigPath, JsonSerializer.Serialize(config, JsonOptions));
        }
    }

    public class TabConfig
    {
        public string Name { get; set; } = "";
        public bool AutoRun { get; set; } = false;
        public string Url { get; set; } = "";
        public string CustomTitle { get; set; } = "";       // 페이지 제목 앞 접두어 (탭 전환 감지용)
        public bool AutoLogin { get; set; } = false;
        public string LoginId { get; set; } = "";           // DPAPI 암호화 저장
        public string LoginPwd { get; set; } = "";          // DPAPI 암호화 저장
        public string IdSelector { get; set; } = "";        // 비어 있으면 자동 탐지
        public string PwdSelector { get; set; } = "";       // 비어 있으면 자동 탐지
        public string SubmitSelector { get; set; } = "";    // 비어 있으면 자동 탐지
        public ExtraInput ExtraInput { get; set; } = new();
        public string TableSelector { get; set; } = "";     // 비어 있으면 자동 탐지 (행이 가장 많은 테이블)
        public string PagingSelector { get; set; } = "";    // 비어 있으면 자동 탐지
    }

    public class ExtraInput
    {
        public string Selector { get; set; } = "";
        /// <summary>text: FillAsync, select: SelectOptionAsync(Label 기준)</summary>
        public string SelectorType { get; set; } = "text";
        public string Value { get; set; } = "";
    }
}
