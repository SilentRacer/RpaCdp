using System.Diagnostics;
using System.Net.Http;

namespace RpaScHauStory
{
    /// <summary>Chrome 디버그 모드 실행 및 CDP 포트 대기를 담당합니다.</summary>
    internal static class ChromeLauncher
    {
        private static readonly string UserDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KTJ", "CDP", "ChromeCdpProfile");

        private static readonly string[] ChromePaths =
        [
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\Application\chrome.exe"),
        ];

        /// <summary>원격 디버깅 포트 9222로 Chrome을 실행합니다.</summary>
        public static void Launch()
        {
            var chromePath = Array.Find(ChromePaths, File.Exists)
                ?? throw new FileNotFoundException("Chrome 실행 파일을 찾을 수 없습니다.");

            Process.Start(new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = $"--remote-debugging-port=9222 --user-data-dir=\"{UserDataDir}\"",
                UseShellExecute = false,
            });
        }

        /// <summary>CDP 엔드포인트가 응답 중인지 확인합니다.</summary>
        public static async Task<bool> IsCdpPortOpenAsync(string endpoint)
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                var resp = await http.GetAsync($"{endpoint}/json/version");
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        /// <summary>CDP 포트가 열릴 때까지 최대 timeoutMs 동안 대기합니다.</summary>
        public static async Task<bool> WaitForPortAsync(string endpoint, int timeoutMs)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                if (await IsCdpPortOpenAsync(endpoint)) return true;
                await Task.Delay(300);
            }
            return false;
        }
    }
}
