using System.Diagnostics;
using System.Net.Http;

namespace RpaScHauStory
{
    public partial class FormMain : Form
    {
        private BrowserTabManager? _tabManager;
        private AppConfig _config = new();

        public FormMain()
        {
            InitializeComponent();
            Load += FormMain_Load;
            FormClosing += FormMain_FormClosing;
            btnSettings.Click += btnSettings_Click;
        }

        // ── 초기화 ────────────────────────────────────────────────

        private async void FormMain_Load(object? sender, EventArgs e)
        {
            _config = AppConfig.Load();
            BuildTabButtons();
            SetTabButtonsEnabled(false);
            await ConnectBrowserAsync();
        }

        private async void FormMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_tabManager is not null)
                await _tabManager.DisposeAsync();
        }

        // ── 탭 버튼 동적 생성 ─────────────────────────────────────

        private void BuildTabButtons()
        {
            pnlTabs.Controls.Clear();

            const int btnHeight = 46;
            const int btnGap = 8;

            foreach (var tab in _config.Tabs)
            {
                var btn = new Button
                {
                    Text = tab.Name,
                    Size = new Size(199, btnHeight),
                    UseVisualStyleBackColor = true,
                    Tag = tab,
                    Margin = new Padding(0, 0, 0, btnGap),
                };
                btn.Click += TabButton_Click;
                pnlTabs.Controls.Add(btn);
            }

            // AutoSize 계산 전에 높이를 직접 설정
            pnlTabs.Size = new Size(199, _config.Tabs.Count * (btnHeight + btnGap));
            lblStatus.Top = pnlTabs.Bottom + 12;
            ClientSize = new Size(ClientSize.Width, lblStatus.Bottom + 20);
        }

        private async void TabButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not TabConfig tab) return;

            await RunTabActionAsync(tab.Name, async () =>
            {
                await _tabManager!.NavigateAsync(tab.Name, tab.Url);
            });
        }

        // ── CDP 연결 ──────────────────────────────────────────────

        private async Task ConnectBrowserAsync()
        {
            try
            {
                lblStatus.Text = "브라우저 확인 중...";

                if (!await IsCdpPortOpenAsync())
                {
                    lblStatus.Text = "Chrome 실행 중...";
                    LaunchChromeWithDebugging();

                    if (!await WaitForCdpPortAsync(timeoutMs: 5000))
                        throw new TimeoutException("Chrome이 5초 내에 응답하지 않았습니다.");
                }

                lblStatus.Text = "브라우저 연결 중...";
                _tabManager = new BrowserTabManager(_config.CdpEndpoint);
                await _tabManager.ConnectAsync();

                lblStatus.Text = "연결됨";
                SetTabButtonsEnabled(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserConnect] {ex}");
                lblStatus.Text = $"연결 실패: {ex.Message}";
                SetTabButtonsEnabled(false);
            }
        }

        // ── Chrome 실행 헬퍼 ──────────────────────────────────────

        private static void LaunchChromeWithDebugging()
        {
            string[] chromePaths =
            [
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Google\Chrome\Application\chrome.exe"),
            ];

            var chromePath = chromePaths.FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException("Chrome 실행 파일을 찾을 수 없습니다.");

            var userDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "KTJ", "CDP", "ChromeCdpProfile");

            Process.Start(new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = $"--remote-debugging-port=9222 --user-data-dir=\"{userDataDir}\"",
                UseShellExecute = false,
            });
        }

        private static async Task<bool> IsCdpPortOpenAsync()
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                var resp = await http.GetAsync($"http://127.0.0.1:9222/json/version");
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> WaitForCdpPortAsync(int timeoutMs)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                if (await IsCdpPortOpenAsync()) return true;
                await Task.Delay(300);
            }
            return false;
        }

        // ── 버튼 핸들러 ───────────────────────────────────────────

        private async void btnSettings_Click(object? sender, EventArgs e)
        {
            using var form = new FormSettings(_config);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _config = AppConfig.Load();
                BuildTabButtons();

                if (_tabManager is not null)
                    await _tabManager.DisposeAsync();

                SetTabButtonsEnabled(false);
                await ConnectBrowserAsync();
            }
        }

        private async void btnConnect_Click(object? sender, EventArgs e)
        {
            if (_tabManager is not null)
                await _tabManager.DisposeAsync();

            SetTabButtonsEnabled(false);
            await ConnectBrowserAsync();
        }

        // ── 헬퍼 ─────────────────────────────────────────────────

        private async Task RunTabActionAsync(string tabName, Func<Task> action)
        {
            if (_tabManager is null || !_tabManager.IsConnected)
            {
                lblStatus.Text = "브라우저가 연결되어 있지 않습니다.";
                return;
            }

            try
            {
                lblStatus.Text = $"[{tabName}] 처리 중...";
                SetTabButtonsEnabled(false);
                await action();
                lblStatus.Text = $"[{tabName}] 완료";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{tabName}] {ex}");
                lblStatus.Text = $"[{tabName}] 오류: {ex.Message}";
            }
            finally
            {
                SetTabButtonsEnabled(true);
            }
        }

        private void SetTabButtonsEnabled(bool enabled)
        {
            foreach (Control ctrl in pnlTabs.Controls)
                ctrl.Enabled = enabled;
        }
    }
}
