using System.Diagnostics;
using Microsoft.Playwright;

namespace RpaScHauStory
{
    public partial class BrowserTabManager
    {
        /// <summary>
        /// 페이지에서 로그인 폼을 자동으로 탐지해 ID/비밀번호를 입력하고 제출합니다.
        /// 로그인 폼이 없거나 이미 로그인된 경우 조용히 종료합니다.
        /// </summary>
        public async Task AutoLoginAsync(IPage page, string loginId, string loginPwd,
            string? idSelector = null, string? pwdSelector = null, string? submitSelector = null,
            ExtraInput? extraInput = null)
        {
            bool hasCredentials = !string.IsNullOrEmpty(loginId) && !string.IsNullOrEmpty(loginPwd);
            ILocator? pwdLocator = null;

            if (hasCredentials)
            {
                // ── 비밀번호 필드 대기 ────────────────────────────────
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

                // ── 추가 입력 (ID 입력 전 처리 — 예: 단지 선택 드롭다운) ──
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
                    string[] candidates =
                    [
                        "input[name='id']", "input[name='userId']", "input[name='loginId']",
                        "input[name='username']", "input[name='user_id']", "input[name='account']",
                        "input[name='email']", "input[type='email']", "input[type='text']",
                    ];
                    foreach (var sel in candidates)
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

            // ── 제출 버튼 ─────────────────────────────────────────────
            if (!string.IsNullOrEmpty(submitSelector))
            {
                var btn = page.Locator(submitSelector).First;
                if (await btn.CountAsync() > 0) { await btn.ClickAsync(); return; }
            }
            else
            {
                string[] fallbacks =
                [
                    "button.login",              // 주택관리사협회: <button class="login">
                    "button:has-text('로그인')",
                    "button:has-text('login')",
                    "button:has-text('Login')",
                    "a:has-text('로그인')",
                    "input[type='submit']",
                ];
                foreach (var sel in fallbacks)
                {
                    var btn     = page.Locator(sel).First;
                    int count   = await btn.CountAsync();
                    bool visible = await btn.IsVisibleAsync();

                    Debug.WriteLine($"[AutoLogin] sel={sel} count={count} visible={visible}");
                    if (count == 0) continue;

                    if (visible)
                    {
                        await btn.ClickAsync();
                        return;
                    }

                    // 주택관리사협회: 버튼이 DOM에 있지만 숨겨진 경우 JS 클릭 시도
                    bool hidden = await btn.IsHiddenAsync();
                    if (!hidden || sel == "button.login")
                    {
                        try
                        {
                            await btn.EvaluateAsync("el => el.click()");
                            return;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[AutoLogin] JS click 실패: {ex.Message}");
                        }
                    }
                }
                Debug.WriteLine("[AutoLogin] 폴백 셀렉터 모두 실패 — Enter 시도");
            }

            if (pwdLocator is not null)
                await pwdLocator.PressAsync("Enter");
        }
    }
}
