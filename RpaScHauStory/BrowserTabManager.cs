using System.Diagnostics;
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
                        State = WaitForSelectorState.Visible,
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
                    var btn = page.Locator(sel).First;
                    int count = await btn.CountAsync();
                    bool visible = await btn.IsVisibleAsync();

                    Debug.WriteLine($"[로그인 체크] sel={sel} count={count} visible={visible}");
                    if (count == 0) continue;            // 셀렉터가 DOM에 없으면 다음으로

                    if (visible)  
                    {
                        await btn.ClickAsync();           // 보이는 버튼이 있으면 클릭
                        Debug.WriteLine($"[로그인 클릭] clicked: {sel}");
                        return;
                    }
                    // visible은 아니지만 DOM에 존재하면 JS 클릭 시도
                    // (특히 첫 번째 selector "button.login"에 유용)   
                    // 주택관리사 협회 때문에 추가했음. 2026.05.17
                    bool hidden = await btn.IsHiddenAsync();
                    if (!hidden || sel == "button.login")  // 또는 항상 fallback
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
            if (_browser is not null)
                await _browser.DisposeAsync();

            _playwright?.Dispose();
        }
    }
}
