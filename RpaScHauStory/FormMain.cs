using System.Diagnostics;

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
        }

        // ── 초기화 ────────────────────────────────────────────────

        private async void FormMain_Load(object? sender, EventArgs e)
        {
            _config = AppConfig.Load();
            BuildTabButtons();
            SetTabButtonsEnabled(false);

            await ConnectBrowserAsync(runAutoRun: true);
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

            const int btnWidth = 200;
            const int btnHeight = 46;
            const int btnGap = 20;

            int cols = Math.Max(1, _config.Columns);

            foreach (var tab in _config.Tabs)
            {
                var btn = new Button
                {
                    Text = tab.Name,
                    Size = new Size(btnWidth, btnHeight),
                    UseVisualStyleBackColor = true,
                    Tag = tab,
                    Margin = new Padding(0, 0, btnGap, btnGap),
                };
                btn.Click += TabButton_Click;
                pnlTabs.Controls.Add(btn);
            }

            int panelWidth = cols * (btnWidth + btnGap);
            int rows = (int)Math.Ceiling(_config.Tabs.Count / (double)cols);
            int panelHeight = Math.Max(1, rows) * (btnHeight + btnGap);
            pnlTabs.Size = new Size(panelWidth, panelHeight);
        }

        private async void TabButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not TabConfig tab) return;

            await RunTabActionAsync(tab.Name, async () =>
            {
                // 이미 해당 URL이 열려 있고 자동 로그온이 켜진 경우 → 포커스만 (재로그온 방지)
                var existingPage = _tabManager!.GetTab(tab.Name);
                if (tab.AutoLogin && existingPage is not null &&
                    string.Equals(existingPage.Url.TrimEnd('/'), tab.Url.TrimEnd('/'),
                        StringComparison.OrdinalIgnoreCase))
                {
                    await existingPage.BringToFrontAsync();
                    return;
                }

                var page = await _tabManager.NavigateAsync(tab.Name, tab.Url,
                    customTitle: string.IsNullOrEmpty(tab.CustomTitle) ? null : tab.CustomTitle);
                if (tab.AutoLogin)
                {
                    lblStatus.Text = $"[{tab.Name}] 자동 로그온 중...";
                    await _tabManager.AutoLoginAsync(page,
                        CredentialHelper.Decrypt(tab.LoginId),
                        CredentialHelper.Decrypt(tab.LoginPwd),
                        tab.IdSelector, tab.PwdSelector, tab.SubmitSelector,
                        tab.ExtraInput);
                }
            });
        }

        // ── CDP 연결 ──────────────────────────────────────────────

        private async Task ConnectBrowserAsync(bool runAutoRun = false)
        {
            try
            {
                lblStatus.Text = "브라우저 확인 중...";

                if (!await ChromeLauncher.IsCdpPortOpenAsync(_config.CdpEndpoint))
                {
                    lblStatus.Text = "Chrome 실행 중...";
                    ChromeLauncher.Launch();

                    if (!await ChromeLauncher.WaitForPortAsync(_config.CdpEndpoint, timeoutMs: 5000))
                        throw new TimeoutException("Chrome이 5초 내에 응답하지 않았습니다.");
                }

                lblStatus.Text = "브라우저 연결 중...";
                _tabManager = new BrowserTabManager(_config.CdpEndpoint);
                await _tabManager.ConnectAsync();
                _tabManager.RegisterTabSwitchHook();

                lblStatus.Text = "연결됨";
                SetTabButtonsEnabled(true);

                if (runAutoRun)
                    await RunAutoTabsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BrowserConnect] {ex}");
                lblStatus.Text = $"연결 실패: {ex.Message}";
                SetTabButtonsEnabled(false);
            }
        }

        private async Task RunAutoTabsAsync()
        {
            var autoTabs = _config.Tabs.Where(t => t.AutoRun).ToList();
            if (autoTabs.Count == 0) return;

            foreach (var tab in autoTabs)
            {
                try
                {
                    lblStatus.Text = $"[자동 실행] {tab.Name}...";
                    var page = await _tabManager!.NavigateAsync(tab.Name, tab.Url,
                        customTitle: string.IsNullOrEmpty(tab.CustomTitle) ? null : tab.CustomTitle);
                    if (tab.AutoLogin && !string.IsNullOrEmpty(tab.LoginId))
                    {
                        lblStatus.Text = $"[자동 로그온] {tab.Name}...";
                        await _tabManager.AutoLoginAsync(page,
                            CredentialHelper.Decrypt(tab.LoginId),
                            CredentialHelper.Decrypt(tab.LoginPwd),
                            tab.IdSelector, tab.PwdSelector, tab.SubmitSelector,
                            tab.ExtraInput);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[AutoRun:{tab.Name}] {ex.Message}");
                }
            }
            lblStatus.Text = "연결됨";
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

        private void FormMain_Load_1(object sender, EventArgs e) { }
    }
}
