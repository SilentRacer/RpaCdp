using System.Text.Json;
using System.Text.Json.Serialization;

namespace RpaScHauStory
{
    public class AppConfig
    {
        public string CdpEndpoint { get; set; } = "http://127.0.0.1:9222";
        public bool AutoConnect { get; set; } = true;
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
