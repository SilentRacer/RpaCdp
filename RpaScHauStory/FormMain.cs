using System.Diagnostics;
using System.Net.Http;
using ClosedXML.Excel;
using Microsoft.Playwright;

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

        private async void btnSaveExcel_Click(object? sender, EventArgs e)
        {
            if (_tabManager is null || !_tabManager.IsConnected)
            {
                lblStatus.Text = "브라우저가 연결되어 있지 않습니다.";
                return;
            }

            btnSaveExcel.Enabled = false;
            try
            {
                lblStatus.Text = "활성 탭 확인 중...";
                await _tabManager.RefreshActivePageAsync();
                var activePage = _tabManager.ActivePage;

                if (activePage is null)
                {
                    lblStatus.Text = "활성 탭을 찾을 수 없습니다. Chrome 탭을 클릭 후 다시 시도하세요.";
                    return;
                }

                var tabConfig = FindTabConfigForPage(activePage);
                var allRows = await ExtractAllPagesAsync(activePage, tabConfig);

                if (allRows.Count == 0)
                {
                    lblStatus.Text = "테이블 데이터를 찾을 수 없습니다.";
                    return;
                }

                using var sfd = new SaveFileDialog
                {
                    Filter = "Excel 파일 (*.xlsx)|*.xlsx",
                    FileName = $"데이터_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                };
                if (sfd.ShowDialog(this) != DialogResult.OK)
                {
                    lblStatus.Text = "저장 취소됨";
                    return;
                }

                SaveToExcel(allRows, sfd.FileName);
                lblStatus.Text = $"저장 완료: {Path.GetFileName(sfd.FileName)} ({allRows.Count - 1}행)";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Excel저장] {ex}");
                lblStatus.Text = $"Excel 저장 오류: {ex.Message}";
            }
            finally
            {
                btnSaveExcel.Enabled = true;
            }
        }

        // ── Excel 저장 헬퍼 ───────────────────────────────────────

        private TabConfig? FindTabConfigForPage(IPage page)
        {
            foreach (var tab in _config.Tabs)
            {
                var registeredPage = _tabManager!.GetTab(tab.Name);
                if (registeredPage == page) return tab;
            }
            // URL 기반 폴백
            foreach (var tab in _config.Tabs)
            {
                if (string.Equals(page.Url.TrimEnd('/'), tab.Url.TrimEnd('/'),
                        StringComparison.OrdinalIgnoreCase))
                    return tab;
            }
            return null;
        }

        private async Task<List<List<string>>> ExtractAllPagesAsync(IPage page, TabConfig? tabConfig)
        {
            var allRows = new List<List<string>>();
            bool isFirstPage = true;
            string prevPageKey = "";

            for (int pageNum = 0; pageNum < 1000; pageNum++)
            {
                lblStatus.Text = $"데이터 추출 중... ({pageNum + 1}페이지)";
                Application.DoEvents();

                var rows = await ExtractTableDataAsync(page, tabConfig?.TableSelector);
                if (rows.Count == 0) break;

                // 직전 페이지와 데이터가 동일하면 마지막 페이지 도달 → 중단
                var pageKey = string.Concat(rows.Take(3).SelectMany(r => r));
                if (pageKey == prevPageKey) break;
                prevPageKey = pageKey;

                if (isFirstPage)
                {
                    allRows.AddRange(rows);
                    isFirstPage = false;
                }
                else
                {
                    // 헤더 행은 첫 페이지에서만 포함
                    allRows.AddRange(rows.Count > 1 ? rows.Skip(1) : rows);
                }

                bool moved = await TryGoToNextPageAsync(page, tabConfig?.PagingSelector);
                if (!moved) break;

                try
                {
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle,
                        new PageWaitForLoadStateOptions { Timeout = 10000 });
                }
                catch { await Task.Delay(1000); }
            }

            return allRows;
        }

        private static async Task<List<List<string>>> ExtractTableDataAsync(IPage page, string? tableSelector)
        {
            var result = await page.EvaluateAsync<string[][]>("""
                selector => {
                    const table = selector
                        ? document.querySelector(selector)
                        : [...document.querySelectorAll('table')]
                            .sort((a, b) => b.rows.length - a.rows.length)[0];
                    if (!table) return [];
                    return [...table.rows].map(row =>
                        [...row.cells].map(cell => cell.innerText.trim().replace(/\s+/g, ' ')));
                }
                """, tableSelector ?? "");

            return result?.Select(r => r.ToList()).ToList() ?? [];
        }

        private static async Task<bool> TryGoToNextPageAsync(IPage page, string? pagingSelector)
        {
            // 페이징 HTML 구조를 디버그로 기록
            var html = await page.EvaluateAsync<string>("""
                selector => {
                    if (selector) {
                        const el = document.querySelector(selector);
                        return el ? el.outerHTML : '(selector not found: ' + selector + ')';
                    }
                    const found = document.querySelector(
                        '[id*="paging"i],[id*="Paging"],[id*="page_"i],[id*="PageNavi"i],' +
                        '[class*="paging"i],[class*="pagination"i],[class*="pageNavi"i]');
                    return found ? found.outerHTML : '(paging container not found)';
                }
                """, pagingSelector ?? "");
            Debug.WriteLine($"[Paging HTML] {html?[..Math.Min(500, html.Length)]}");

            return await page.EvaluateAsync<bool>("""
                selector => {
                    // ① 페이징 컨테이너 탐지
                    let paging = selector ? document.querySelector(selector) : null;

                    if (!paging) {
                        // id/class 키워드 매칭
                        paging = document.querySelector(
                            '[id*="paging"i],[id*="Paging"],[id*="PageNavi"i],[id*="pageNum"i],' +
                            '[class*="paging"i],[class*="pagination"i],[class*="pageNavi"i]');
                    }

                    if (!paging) {
                        // 숫자 링크 3개 이상 있는 최소 컨테이너 탐지
                        const candidates = [...document.querySelectorAll('div,td,tr,ul,nav,p,span,table')];
                        let best = null, bestCount = 0;
                        for (const el of candidates) {
                            const nums = [...el.querySelectorAll('a')]
                                .map(a => parseInt(a.textContent.trim()))
                                .filter(n => Number.isInteger(n) && n >= 1 && n < 10000);
                            // 중복 제거 후 고유 숫자 수
                            const uniq = new Set(nums).size;
                            if (uniq >= 3 && uniq > bestCount) {
                                bestCount = uniq;
                                best = el;
                            }
                        }
                        paging = best;
                    }

                    if (!paging) return false;

                    // ② 현재 페이지 번호 탐지
                    let currentPage = NaN;

                    // strong / b / em 또는 .on .active .current .selected 계열
                    const activeEl = paging.querySelector(
                        'strong, b, em, .on, .active, .current, .selected,' +
                        '[class*="on"],[class*="cur"],[class*="active"],[class*="select"]');
                    if (activeEl) {
                        const n = parseInt(activeEl.textContent.trim());
                        if (!isNaN(n)) currentPage = n;
                    }

                    // 링크가 없는 숫자 span/td/li
                    if (isNaN(currentPage)) {
                        const nonLinks = [...paging.querySelectorAll('span,td,li,div')]
                            .filter(el => !el.querySelector('a') && el.tagName !== 'A');
                        for (const el of nonLinks) {
                            const n = parseInt(el.textContent.trim());
                            if (!isNaN(n) && n >= 1) { currentPage = n; break; }
                        }
                    }

                    // ③ 다음 번호 링크 클릭
                    const allLinks = [...paging.querySelectorAll('a')];
                    if (!isNaN(currentPage)) {
                        const nextLink = allLinks.find(a => parseInt(a.textContent.trim()) === currentPage + 1);
                        if (nextLink) { nextLink.click(); return true; }
                    }

                    // ④ "다음" / ">" / "▶" 텍스트 링크
                    const nextBtn = allLinks.find(a => /^(다음|next|>|▶|»|>)$/i.test(a.textContent.trim()));
                    if (nextBtn) { nextBtn.click(); return true; }

                    return false;
                }
                """, pagingSelector ?? "");
        }

        private static void SaveToExcel(List<List<string>> rows, string filePath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("데이터");

            for (int r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                for (int c = 0; c < row.Count; c++)
                    ws.Cell(r + 1, c + 1).Value = row[c];
            }

            if (rows.Count > 0)
            {
                var headerRow = ws.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(189, 215, 238);
            }

            ws.Columns().AdjustToContents(1, 60);
            wb.SaveAs(filePath);
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
