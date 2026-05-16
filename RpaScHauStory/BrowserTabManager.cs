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
        private readonly string _cdpEndpoint;

        public bool IsConnected => _browser?.IsConnected ?? false;
        public IReadOnlyDictionary<string, IPage> Tabs => _tabs;

        public BrowserTabManager(string cdpEndpoint = "http://127.0.0.1:9222")  // 기본 CDP 엔드포인트는 localhost:9222 대신 ip주소로 변경
        {
            _cdpEndpoint = cdpEndpoint;
        }

        /// <summary>CDP 엔드포인트로 이미 실행 중인 브라우저에 연결합니다.</summary>
        public async Task ConnectAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.ConnectOverCDPAsync(_cdpEndpoint);
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

            if (url is not null)
                await page.GotoAsync(url);

            return page;
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
        public async Task<IPage> NavigateAsync(string tabName, string url,
            WaitUntilState waitUntil = WaitUntilState.Load)
        {
            var page = await GetOrCreateTabAsync(tabName);
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = waitUntil });
            await page.BringToFrontAsync();
            return page;
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
            if (_browser is not null)
                await _browser.DisposeAsync();

            _playwright?.Dispose();
        }
    }
}
