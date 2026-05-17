using System.Text.Json;
using System.Text.Json.Serialization;

namespace RpaScHauStory
{
    public class AppConfig
    {
        public string CdpEndpoint { get; set; } = "http://127.0.0.1:9222";
        public int Columns { get; set; } = 1;
        public List<TabConfig> Tabs { get; set; } = [];

        // 사용자 설정 저장 경로 (빌드와 무관하게 유지됨)
        private static readonly string UserConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KTJ", "CDP", "appsettings.json");

        // 앱 설치 폴더의 기본 설정 (최초 실행 또는 사용자 설정 없을 때 사용)
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
            // 사용자 설정 파일이 있으면 우선 사용, 없으면 기본 설정 사용
            var path = File.Exists(UserConfigPath) ? UserConfigPath : DefaultConfigPath;
            if (!File.Exists(path)) return new AppConfig();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }

        public static void Save(AppConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(UserConfigPath)!);
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(UserConfigPath, json);
        }
    }

    public class TabConfig
    {
        public string Name { get; set; } = "";
        public bool AutoRun { get; set; } = false;
        public string Url { get; set; } = "";
        public bool AutoLogin { get; set; } = false;
        public string LoginId { get; set; } = "";     // DPAPI 암호화 저장
        public string LoginPwd { get; set; } = "";    // DPAPI 암호화 저장
        public string IdSelector { get; set; } = "";  // 비어 있으면 자동 탐지
        public string PwdSelector { get; set; } = ""; // 비어 있으면 자동 탐지
        public string SubmitSelector { get; set; } = ""; // 비어 있으면 자동 탐지
        public ExtraInput ExtraInput { get; set; } = new();
    }

    public class ExtraInput
    {
        public string Selector { get; set; } = "";
        /// <summary>text: FillAsync, select: SelectOptionAsync(Label 기준)</summary>
        public string SelectorType { get; set; } = "text";
        public string Value { get; set; } = "";
    }

    public class FormSettingsLayout
    {
        public bool UseUserLayout { get; set; } = false;
        public int FormWidth { get; set; } = 900;
        public int FormHeight { get; set; } = 768;
        public Dictionary<string, int> ColumnWidths { get; set; } = new();

        private static readonly string LayoutPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KTJ", "CDP", "formSettings.layout.json");

        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public static FormSettingsLayout Load()
        {
            if (!File.Exists(LayoutPath)) return new FormSettingsLayout();
            try
            {
                var json = File.ReadAllText(LayoutPath);
                return JsonSerializer.Deserialize<FormSettingsLayout>(json, JsonOpts) ?? new();
            }
            catch { return new FormSettingsLayout(); }
        }

        public static void Save(FormSettingsLayout layout)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LayoutPath)!);
            File.WriteAllText(LayoutPath, JsonSerializer.Serialize(layout, JsonOpts));
        }
    }

    public static class CredentialHelper
    {
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            var encrypted = System.Security.Cryptography.ProtectedData.Protect(
                bytes, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return "";
            try
            {
                var bytes = Convert.FromBase64String(encryptedText);
                var decrypted = System.Security.Cryptography.ProtectedData.Unprotect(
                    bytes, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return System.Text.Encoding.UTF8.GetString(decrypted);
            }
            catch { return ""; }
        }
    }
}
