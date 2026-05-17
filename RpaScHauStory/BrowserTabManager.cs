using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Playwright;

namespace RpaScHauStory
{
    /// <summary>
    /// CDP로 연결된 브라우저의 탭을 이름 기반으로 관리합니다.
    /// 사전에 Chrome을 --remote-debugging-port=9222 옵션으로 실행해야 합니다.
    /// </summary>
    public class BrowserTabManager : IAsyncDisposable
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private readonly Dictionary<string, IPage> _tabs = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<IPage, (string Title, EventHandler<IPage> Handler)> _customTitleHandlers = new();
        private readonly string _cdpEndpoint;

        public bool IsConnected => _browser?.IsConnected ?? false;
        public IReadOnlyDictionary<string, IPage> Tabs => _tabs;

        /// <summary>현재 Chrome에서 사용자가 보고 있는 활성 탭입니다.</summary>
        public IPage? ActivePage { get; private set; }

        public BrowserTabManager(string cdpEndpoint = "http://127.0.0.1:9222")
        {
            _cdpEndpoint = cdpEndpoint;
        }

        // ── 탭 전환 감지: Windows WinEvent 훅 ───────────────────────
        // Chrome은 --disable-renderer-backgrounding으로 모든 탭을 항상 active 상태로 유지하므로
        // visibilitychange/focus JS 이벤트와 CDP 이벤트로는 탭 전환을 감지할 수 없음.
        // 대신 Chrome은 탭 전환 시 창 제목을 변경하므로 EVENT_OBJECT_NAMECHANGE 훅으로 감지한다.

        private delegate void WinEventDelegate(IntPtr hook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint thread, uint time);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
            IntPtr hmodWinEventProc, WinEventDelegate proc,
            uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hook);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        private const uint WINEVENT_OUTOFCONTEXT   = 0x0000;

        // GC 수거 방지용 static 참조 (콜백 델리게이트는 반드시 static으로 유지)
        private static BrowserTabManager? _current;
        private static IntPtr             _winEventHook;
        private static WinEventDelegate?  _winEventProc;

        /// <summary>
        /// WinEvent 훅을 등록합니다. UI 스레드(메시지 루프가 있는 스레드)에서 호출하세요.
        /// </summary>
        public void RegisterTabSwitchHook()
        {
            UnregisterTabSwitchHook();
            _current      = this;
            _winEventProc = OnWindowNameChanged;
            _winEventHook = SetWinEventHook(
                EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE,
                IntPtr.Zero, _winEventProc,
                0, 0, WINEVENT_OUTOFCONTEXT);
            Debug.WriteLine("[TabHook] WinEvent 훅 등록됨");
        }

        public void UnregisterTabSwitchHook()
        {
            if (_winEventHook != IntPtr.Zero)
            {
                UnhookWinEvent(_winEventHook);
                _winEventHook = IntPtr.Zero;
                Debug.WriteLine("[TabHook] WinEvent 훅 해제됨");
            }
        }

        private static void OnWindowNameChanged(IntPtr hook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint thread, uint time)
        {
            if (idObject != 0) return; // OBJID_WINDOW(0)만 처리

            var sb = new StringBuilder(256);
            GetWindowText(hwnd, sb, sb.Capacity);
            var title = sb.ToString();
            if (!title.Contains("Chrome", StringComparison.OrdinalIgnoreCase)) return;

            Debug.WriteLine($"[TabHook] 탭 전환 감지: {title}");
            _ = _current?.UpdateActivePageAsync(title);
        }

        /// <summary>창 제목을 각 페이지 title과 비교해 ActivePage를 설정합니다.</summary>
        private async Task UpdateActivePageAsync(string windowTitle)
        {
            foreach (var page in GetAllPages())
            {
                try
                {
                    var pageTitle = await page.TitleAsync();
                    if (!string.IsNullOrEmpty(pageTitle) &&
                        windowTitle.Contains(pageTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        ActivePage = page;
                        Debug.WriteLine($"[TabHook] ActivePage → {page.Url}");
                        return;
                    }
                }
                catch { }
            }
        }

        /// <summary>현재 포그라운드 창 제목으로 ActivePage를 즉시 갱신합니다.</summary>
        public async Task RefreshActivePageAsync()
        {
            var hwnd = GetForegroundWindow();
            var sb = new StringBuilder(512);
            GetWindowText(hwnd, sb, sb.Capacity);
            await UpdateActivePageAsync(sb.ToString());
        }

        // ── CDP 연결 ──────────────────────────────────────────────

        /// <summary>CDP 엔드포인트로 이미 실행 중인 브라우저에 연결합니다.</summary>
        public async Task ConnectAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser    = await _playwright.Chromium.ConnectOverCDPAsync(_cdpEndpoint);
        }

        /// <summary>
        /// 이름으로 탭을 가져옵니다. 없으면 새 탭을 열고 등록합니다.
        /// url을 전달하면 탭 생성 직후 해당 URL로 이동합니다.
        /// </summary>
        public async Task<IPage> GetOrCreateTabAsync(string tabName, string? url = null)
        {
            EnsureConnected();

            if (_tabs.TryGetValue(tabName, out var existing) && !existing.IsClosed)
                return existing;

            var context = _browser!.Contexts.FirstOrDefault()
                ?? await _browser.NewContextAsync();

            var page = await context.NewPageAsync();
            _tabs[tabName] = page;

            page.Close += (_, _) => _tabs.Remove(tabName);
            page.Close += (_, _) => { if (_customTitleHandlers.TryGetValue(page, out var h)) { page.Load -= h.Handler; _customTitleHandlers.Remove(page); } };

            if (url is not null)
                await page.GotoAsync(url);

            return page;
        }

        /// <summary>브라우저에 열린 모든 페이지(앱 등록 여부 무관)를 반환합니다.</summary>
        public IReadOnlyList<IPage> GetAllPages()
        {
            if (_browser is null) return [];
            return _browser.Contexts
                .SelectMany(ctx => ctx.Pages)
                .Where(p => !p.IsClosed)
                .ToList();
        }

        /// <summary>탭 이름으로 기존 탭을 반환합니다. 없으면 null.</summary>
        public IPage? GetTab(string tabName)
        {
            _tabs.TryGetValue(tabName, out var page);
            return page is { IsClosed: false } ? page : null;
        }

        /// <summary>탭을 앞으로 가져옵니다 (BringToFront).</summary>
        public async Task FocusTabAsync(string tabName)
        {
            if (GetTab(tabName) is { } page)
                await page.BringToFrontAsync();
        }

        /// <summary>탭을 지정 URL로 이동합니다. 탭이 없으면 새로 생성합니다.</summary>
        /// <param name="customTitle">비어 있지 않으면 페이지 로드 후 document.title을 이 값으로 강제 설정합니다.</param>
        public async Task<IPage> NavigateAsync(string tabName, string url,
            WaitUntilState waitUntil = WaitUntilState.Load,
            string? customTitle = null)
        {
            var page = await GetOrCreateTabAsync(tabName);
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = waitUntil });
            await page.BringToFrontAsync();
            if (!string.IsNullOrEmpty(customTitle))
                await SetCustomTitleAsync(page, customTitle);
            return page;
        }

        /// <summary>
        /// 페이지 제목 앞에 접두어를 붙이고 MutationObserver로 고정합니다.
        /// 페이지 자체 JS가 제목을 바꾸면 즉시 접두어를 재적용합니다.
        /// </summary>
        public async Task SetCustomTitleAsync(IPage page, string customTitle)
        {
            // 이전 Load 핸들러 제거 (동일 탭에 대한 중복 등록 방지)
            if (_customTitleHandlers.TryGetValue(page, out var existing))
            {
                page.Load -= existing.Handler;
                _customTitleHandlers.Remove(page);
            }

            await ApplyCustomTitleScriptAsync(page, customTitle);

            // 페이지 내 JS 리다이렉트 등 이후 navigation에서도 재적용
            EventHandler<IPage> handler = async (_, _) =>
            {
                try { await ApplyCustomTitleScriptAsync(page, customTitle); } catch { }
            };
            page.Load += handler;
            _customTitleHandlers[page] = (customTitle, handler);
        }

        private static async Task ApplyCustomTitleScriptAsync(IPage page, string customTitle)
        {
            try
            {
                await page.EvaluateAsync("""
                    prefix => {
                        if (window.__rpaTitleObserver) window.__rpaTitleObserver.disconnect();

                        const apply = () => {
                            if (document.title && !document.title.startsWith(prefix))
                                document.title = prefix + ' ' + document.title;
                        };

                        apply();

                        // <head> 전체를 감시해 <title> 요소 교체도 감지
                        const obs = new MutationObserver(apply);
                        const target = document.querySelector('head') || document.documentElement;
                        obs.observe(target, { childList: true, subtree: true, characterData: true });
                        window.__rpaTitleObserver = obs;
                    }
                    """, customTitle);
            }
            catch { }
        }

        /// <summary>
        /// 페이지에서 로그인 폼을 자동으로 탐지해 ID/비밀번호를 입력하고 제출합니다.
        /// 로그인 폼이 없거나 이미 로그인된 경우 조용히 종료합니다.
        /// </summary>
        /// <param name="idSelector">CSS 셀렉터 — null/empty면 자동 탐지</param>
        /// <param name="pwdSelector">CSS 셀렉터 — null/empty면 자동 탐지</param>
        /// <param name="submitSelector">CSS 셀렉터 — null/empty면 자동 탐지</param>
        public async Task AutoLoginAsync(IPage page, string loginId, string loginPwd,
            string? idSelector = null, string? pwdSelector = null, string? submitSelector = null,
            ExtraInput? extraInput = null)
        {
            bool hasCredentials = !string.IsNullOrEmpty(loginId) && !string.IsNullOrEmpty(loginPwd);
            ILocator? pwdLocator = null;

            if (hasCredentials)
            {
                // ── 비밀번호 필드 ─────────────────────────────────────
                pwdLocator = string.IsNullOrEmpty(pwdSelector)
                    ? page.Locator("input[type='password']").First
                    : page.Locator(pwdSelector).First;

                try
                {
                    await pwdLocator.WaitForAsync(new LocatorWaitForOptions
                    {
                        State   = WaitForSelectorState.Visible,
                        Timeout = 5000,
                    });
                }
                catch (TimeoutException)
                {
                    return; // 로그인 폼 없음 또는 이미 로그인된 상태
                }

                // ── 추가 입력 필드 (ID 입력 전 처리 — 예: 단지 선택 드롭다운) ──
                if (extraInput is not null && !string.IsNullOrEmpty(extraInput.Selector))
                {
                    var extraField = page.Locator($"select[name='{extraInput.Selector}']").First;
                    if (await extraField.CountAsync() > 0 && await extraField.IsVisibleAsync())
                    {
                        if (string.Equals(extraInput.SelectorType, "select", StringComparison.OrdinalIgnoreCase))
                            await extraField.SelectOptionAsync(new SelectOptionValue { Label = extraInput.Value });
                        else
                            await extraField.FillAsync(extraInput.Value);
                    }
                }

                // ── ID 필드 ───────────────────────────────────────────
                if (!string.IsNullOrEmpty(idSelector))
                {
                    var field = page.Locator(idSelector).First;
                    if (await field.CountAsync() > 0)
                        await field.FillAsync(loginId);
                }
                else
                {
                    string[] fallbackIdSelectors =
                    [
                        "input[name='id']",
                        "input[name='userId']",
                        "input[name='loginId']",
                        "input[name='username']",
                        "input[name='user_id']",
                        "input[name='account']",
                        "input[name='email']",
                        "input[type='email']",
                        "input[type='text']",
                    ];
                    foreach (var sel in fallbackIdSelectors)
                    {
                        var field = page.Locator(sel).First;
                        if (await field.CountAsync() > 0 && await field.IsVisibleAsync())
                        {
                            await field.FillAsync(loginId);
                            break;
                        }
                    }
                }

                await pwdLocator.FillAsync(loginPwd);
            }

            // ── 제출 버튼 ─────────────────────────────────────────
            if (!string.IsNullOrEmpty(submitSelector))
            {
                var btn = page.Locator(submitSelector).First;
                if (await btn.CountAsync() > 0)
                {
                    await btn.ClickAsync();
                    return;
                }
            }
            else
            {
                string[] fallbackSubmitSelectors =
                [
                    "button.login",                 // 주택관리사 협회 로그인 버튼이 여기에 해당  <button class="login">
                    "button:has-text('로그인')",
                    "button:has-text('login')",
                    "button:has-text('Login')",
                    "a:has-text('로그인')",
                    "input[type='submit']",
                    //"button[type='submit']",      // 가장 일반적인 제출 버튼 그런데 주관사는 검색버튼이라서 주석처리
                ];
                Debug.WriteLine($"[로그인 체크~~~ ] {page.Url}");
                foreach (var sel in fallbackSubmitSelectors)
                {
                    var btn     = page.Locator(sel).First;
                    int count   = await btn.CountAsync();
                    bool visible = await btn.IsVisibleAsync();

                    Debug.WriteLine($"[로그인 체크] sel={sel} count={count} visible={visible}");
                    if (count == 0) continue;

                    if (visible)
                    {
                        await btn.ClickAsync();
                        Debug.WriteLine($"[로그인 클릭] clicked: {sel}");
                        return;
                    }
                    // visible은 아니지만 DOM에 존재하면 JS 클릭 시도
                    // 주택관리사 협회 때문에 추가했음. 2026.05.17
                    bool hidden = await btn.IsHiddenAsync();
                    if (!hidden || sel == "button.login")
                    {
                        try
                        {
                            await btn.EvaluateAsync("el => el.click()");
                            Debug.WriteLine($"[로그인 JS 클릭] JS-clicked (was invisible): {sel}");
                            return;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[고르인 JS 오류] JS click failed: {ex.Message}");
                        }
                    }
                }
                Debug.WriteLine("[Submit] 폴백 셀렉터 모두 실패 — Enter 시도");
            }

            // 제출 버튼을 찾지 못하면 Enter (자격증명이 있는 경우만)
            if (pwdLocator is not null)
                await pwdLocator.PressAsync("Enter");
        }

        /// <summary>이름으로 탭을 닫습니다.</summary>
        public async Task CloseTabAsync(string tabName)
        {
            if (_tabs.TryGetValue(tabName, out var page))
            {
                _tabs.Remove(tabName);
                if (!page.IsClosed)
                    await page.CloseAsync();
            }
        }

        /// <summary>
        /// 현재 브라우저에 열린 실제 탭 목록을 동기화합니다.
        /// ConnectOverCDP 직후 기존 탭을 이름 없이 인덱스로 등록할 때 사용합니다.
        /// </summary>
        public async Task SyncExistingTabsAsync()
        {
            EnsureConnected();
            var context = _browser!.Contexts.FirstOrDefault();
            if (context is null) return;

            var pages = context.Pages;
            for (int i = 0; i < pages.Count; i++)
            {
                var name = $"tab_{i}";
                if (!_tabs.ContainsKey(name))
                    _tabs[name] = pages[i];
            }
            await Task.CompletedTask;
        }

        private void EnsureConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException("브라우저에 연결되어 있지 않습니다. ConnectAsync()를 먼저 호출하세요.");
        }

        public async ValueTask DisposeAsync()
        {
            UnregisterTabSwitchHook();

            if (_browser is not null)
                await _browser.DisposeAsync();

            _playwright?.Dispose();
        }
    }
}