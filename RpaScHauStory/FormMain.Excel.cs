using System.Diagnostics;
using ClosedXML.Excel;
using Microsoft.Playwright;

namespace RpaScHauStory
{
    public partial class FormMain
    {
        // ── Excel 저장 버튼 핸들러 ────────────────────────────────

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

        // ── 페이지 → 탭 설정 매핑 ────────────────────────────────

        private TabConfig? FindTabConfigForPage(IPage page)
        {
            foreach (var tab in _config.Tabs)
            {
                var registeredPage = _tabManager!.GetTab(tab.Name);
                if (registeredPage == page) return tab;
            }
            foreach (var tab in _config.Tabs)
            {
                if (string.Equals(page.Url.TrimEnd('/'), tab.Url.TrimEnd('/'),
                        StringComparison.OrdinalIgnoreCase))
                    return tab;
            }
            return null;
        }

        // ── 전체 페이지 데이터 수집 ───────────────────────────────

        private async Task<List<List<string>>> ExtractAllPagesAsync(IPage page, TabConfig? tabConfig)
        {
            var allRows = new List<List<string>>();
            var visitedKeys = new HashSet<string>();

            for (int pageNum = 0; pageNum < 1000; pageNum++)
            {
                lblStatus.Text = $"데이터 추출 중... ({pageNum + 1}페이지)";
                Application.DoEvents();

                var rows = await ExtractTableDataAsync(page, tabConfig?.TableSelector);
                if (rows.Count == 0) break;

                var pageKey = string.Concat(rows.Take(3).SelectMany(r => r));
                if (!visitedKeys.Add(pageKey)) break;

                if (pageNum == 0)
                    allRows.AddRange(rows);
                else
                    allRows.AddRange(rows.Count > 1 ? rows.Skip(1) : rows);

                var (moved, diagnosis) = await TryGoToNextPageAsync(page, tabConfig?.PagingSelector);
                Debug.WriteLine($"[Paging] {diagnosis}");
                if (!moved)
                {
                    lblStatus.Text += diagnosis.Contains("다음링크 없음")
                        ? " → 마지막 페이지 수집 완료"
                        : $" | {diagnosis}";
                    break;
                }

                await WaitForNewPageAsync(page, tabConfig?.TableSelector, visitedKeys);
            }

            return allRows;
        }

        /// <summary>
        /// 방문한 적 없는 새 페이지 데이터가 안정적으로 나타날 때까지 최대 10초 대기합니다.
        /// 동일한 새 키가 2회 연속 확인될 때 종료해 포스트백 과도기 오인을 방지합니다.
        /// </summary>
        private static async Task WaitForNewPageAsync(
            IPage page, string? tableSelector, HashSet<string> visitedKeys)
        {
            try
            {
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle,
                    new PageWaitForLoadStateOptions { Timeout = 10000 });
            }
            catch { }

            await Task.Delay(300);

            var deadline = DateTime.UtcNow.AddSeconds(8);
            string stableKey = "";
            while (DateTime.UtcNow < deadline)
            {
                var rows = await ExtractTableDataAsync(page, tableSelector);
                var key = string.Concat(rows.Take(3).SelectMany(r => r));
                if (rows.Count > 1 && !visitedKeys.Contains(key))
                {
                    if (key == stableKey) break;
                    stableKey = key;
                }
                else
                {
                    stableKey = "";
                }
                await Task.Delay(300);
            }
        }

        // ── 테이블 데이터 추출 ────────────────────────────────────

        private static async Task<List<List<string>>> ExtractTableDataAsync(
            IPage page, string? tableSelector)
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

        // ── 페이지 이동 ───────────────────────────────────────────

        private static async Task<(bool Moved, string Diagnosis)> TryGoToNextPageAsync(
            IPage page, string? pagingSelector)
        {
            var result = await page.EvaluateAsync<string>("""
                selector => {
                    // ── 페이징 컨테이너 탐지 ──────────────────────────────

                    let paging = selector ? document.querySelector(selector) : null;
                    let method = selector ? 'selector' : '';

                    // 방법 1: id/class 키워드
                    if (!paging) {
                        paging = document.querySelector(
                            '[id*="paging"i],[id*="Paging"],[id*="PageNavi"i],[id*="pageNum"i],[id*="paginate"i],' +
                            '[class*="paging"i],[class*="pagination"i],[class*="pageNavi"i],[class*="paginate"i]');
                        if (paging) method = 'keyword:' + (paging.id || paging.className).slice(0, 60);
                    }

                    // 방법 2: 가장 큰 테이블의 마지막 행 (ASP.NET GridView 패턴)
                    if (!paging) {
                        const tables = [...document.querySelectorAll('table')]
                            .sort((a, b) => b.rows.length - a.rows.length);
                        for (const tbl of tables.slice(0, 5)) {
                            const last = tbl.rows[tbl.rows.length - 1];
                            if (!last) continue;
                            const numEls = [...last.querySelectorAll('a,span,b,strong,td')]
                                .filter(el => /^\d+$/.test(el.textContent.trim()));
                            if (numEls.length >= 2) { paging = last; method = 'gridview-last-row'; break; }
                        }
                    }

                    // 방법 3: 숫자 링크 2개 이상인 최소 컨테이너
                    if (!paging) {
                        let best = null, bestCount = 0;
                        for (const el of document.querySelectorAll('div,td,tr,ul,nav,p,span,table,tbody')) {
                            const links = [...el.querySelectorAll('a')]
                                .filter(a => /^\d+$/.test(a.textContent.trim()));
                            const uniq = new Set(links.map(a => parseInt(a.textContent))).size;
                            if (uniq >= 2 && uniq > bestCount) { bestCount = uniq; best = el; }
                        }
                        if (best) { paging = best; method = 'numlink:' + bestCount; }
                    }

                    if (!paging) return 'fail:컨테이너 없음';

                    // ── 현재 페이지 번호 탐지 ────────────────────────────

                    let currentPage = NaN;

                    const activeEl = paging.querySelector(
                        'strong,b,em,.on,.active,.current,.selected,' +
                        '[class*="on"],[class*="cur"],[class*="active"],[class*="select"]');
                    if (activeEl) {
                        const n = parseInt(activeEl.textContent.trim());
                        if (!isNaN(n)) currentPage = n;
                    }

                    if (isNaN(currentPage)) {
                        for (const el of paging.querySelectorAll('*')) {
                            if (el.tagName === 'A' || el.querySelector('a')) continue;
                            const n = parseInt(el.textContent.trim());
                            if (!isNaN(n) && n >= 1 && n < 10000) { currentPage = n; break; }
                        }
                    }

                    // ── 다음 페이지 클릭 ────────────────────────────────

                    const allLinks = [...paging.querySelectorAll('a')];

                    if (!isNaN(currentPage)) {
                        const nextLink = allLinks.find(a => parseInt(a.textContent.trim()) === currentPage + 1);
                        if (nextLink) { nextLink.click(); return 'ok:' + (currentPage + 1) + '(' + method + ')'; }
                    }

                    // "다음" 버튼: 텍스트·title·img alt·img src 모두 검사
                    const isNext = a => {
                        const text  = a.textContent.trim();
                        const title = (a.title || a.getAttribute('aria-label') || '').trim();
                        const img   = a.querySelector('img');
                        const alt   = (img?.alt || '').trim();
                        const src   = (img?.src || img?.getAttribute('src') || '').toLowerCase();
                        const combined = text + title + alt;
                        if (/^\d+$/.test(text)) return false;
                        return /다음|next/i.test(combined) || /next/i.test(src) || /^[>▶»›]$/.test(text);
                    };
                    const nextBtn = allLinks.find(isNext);
                    if (nextBtn) {
                        const img = nextBtn.querySelector('img');
                        const label = nextBtn.textContent.trim()
                            || nextBtn.title
                            || (img?.alt || '')
                            || (img?.src || '').split('/').pop()?.split('?')[0]
                            || '?';
                        nextBtn.click();
                        return 'ok:next[' + label + '](' + method + ')';
                    }

                    const linkTexts = allLinks.map(a => '"' + a.textContent.trim() + '"').join(',');
                    return 'fail:다음링크 없음(현재=' + currentPage + ',링크=[' + linkTexts + '],방법=' + method + ')';
                }
                """, pagingSelector ?? "");

            if (result?.StartsWith("ok:") == true)
                return (true, result);
            return (false, result ?? "fail:null");
        }

        // ── Excel 파일 저장 ───────────────────────────────────────

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
    }
}
