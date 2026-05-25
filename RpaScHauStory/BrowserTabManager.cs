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
    public partial class BrowserTabManager : IAsyncDisposable
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
                        if (ActivePage != page)
                        {
                            var prev = ActivePage?.Url ?? "(없음)";
                            ActivePage = page;
                            Debug.WriteLine($"[ActivePage 변경] {prev}  \n→  {page.Url}  (제목: {pageTitle})");
                        }
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