namespace RpaScHauStory
{
    public partial class FormSettings : Form
    {
        private readonly AppConfig _config;
        private FormSettingsLayout _layout = new();

        // ── 개발자 기본 레이아웃 ──────────────────────────────────
        private const int DefaultFormWidth  = 900;
        private const int DefaultFormHeight = 768;

        private static readonly (string Name, int Width)[] DefaultColumnWidths =
        [
            ("colName",            130),
            ("colAutoRun",          80),
            ("colUrl",             300),
            ("colAutoLogin",        90),
            ("colLoginId",         150),
            ("colLoginPwd",        150),
            ("colIdSelector",      200),
            ("colPwdSelector",     200),
            ("colSubmitSelector",  200),
            ("colExtraSelector",   200),
            ("colExtraSelectorType", 100),
            ("colExtraValue",      150),
        ];

        public FormSettings(AppConfig config)
        {
            InitializeComponent();
            _config = config;

            Load    += FormSettings_Load;
            FormClosed += FormSettings_FormClosed;
            btnAdd.Click      += btnAdd_Click;
            btnDelete.Click   += btnDelete_Click;
            btnMoveUp.Click   += btnMoveUp_Click;
            btnMoveDown.Click += btnMoveDown_Click;
            btnSave.Click     += btnSave_Click;
            btnCancel.Click   += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            chkUserLayout.CheckedChanged += chkUserLayout_CheckedChanged;
        }

        // ── 초기화 ────────────────────────────────────────────────

        private void FormSettings_Load(object? sender, EventArgs e)
        {
            _layout = FormSettingsLayout.Load();
            chkUserLayout.Checked = _layout.UseUserLayout;

            if (_layout.UseUserLayout && _layout.ColumnWidths.Count > 0)
                ApplyUserLayout();
            else
                ApplyDefaultLayout();

            txtCdpEndpoint.Text = _config.CdpEndpoint;
            chkAutoConnect.Checked = _config.AutoConnect;
            nudColumns.Value = Math.Max(nudColumns.Minimum, Math.Min(nudColumns.Maximum, _config.Columns));

            foreach (var tab in _config.Tabs)
                dgvTabs.Rows.Add(
                    tab.Name, tab.AutoRun, tab.Url,
                    tab.AutoLogin,
                    CredentialHelper.Decrypt(tab.LoginId),
                    CredentialHelper.Decrypt(tab.LoginPwd),
                    tab.IdSelector, tab.PwdSelector, tab.SubmitSelector,
                    tab.ExtraInput.Selector,
                    string.IsNullOrEmpty(tab.ExtraInput.SelectorType) ? "text" : tab.ExtraInput.SelectorType,
                    tab.ExtraInput.Value);
        }

        private void FormSettings_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _layout.UseUserLayout = chkUserLayout.Checked;
            if (_layout.UseUserLayout)
            {
                _layout.FormWidth  = ClientSize.Width;
                _layout.FormHeight = ClientSize.Height;
                _layout.ColumnWidths.Clear();
                foreach (DataGridViewColumn col in dgvTabs.Columns)
                    _layout.ColumnWidths[col.Name] = col.Width;
            }
            FormSettingsLayout.Save(_layout);
        }

        // ── 레이아웃 적용 ─────────────────────────────────────────

        private void ApplyDefaultLayout()
        {
            ClientSize = new Size(DefaultFormWidth, DefaultFormHeight);
            foreach (var (name, width) in DefaultColumnWidths)
            {
                if (dgvTabs.Columns[name] is { } col)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    col.Width = width;
                }
            }
        }

        private void ApplyUserLayout()
        {
            if (_layout.FormWidth > 0 && _layout.FormHeight > 0)
                ClientSize = new Size(_layout.FormWidth, _layout.FormHeight);

            foreach (var (name, width) in _layout.ColumnWidths)
            {
                if (dgvTabs.Columns[name] is { } col)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    col.Width = width;
                }
            }
        }

        private void chkUserLayout_CheckedChanged(object? sender, EventArgs e)
        {
            if (!chkUserLayout.Checked)
                ApplyDefaultLayout();
        }

        // ── 버튼 핸들러 ───────────────────────────────────────────

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            int idx = dgvTabs.Rows.Add("새 버튼", false, "https://", false, "", "", "", "", "", "", "text", "");
            dgvTabs.ClearSelection();
            dgvTabs.Rows[idx].Selected = true;
            dgvTabs.CurrentCell = dgvTabs.Rows[idx].Cells[0];
            dgvTabs.BeginEdit(true);
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvTabs.SelectedRows.Count == 0) return;

            var result = MessageBox.Show(
                "선택한 항목을 삭제하시겠습니까?",
                "삭제 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                dgvTabs.Rows.RemoveAt(dgvTabs.SelectedRows[0].Index);
        }

        private void btnMoveUp_Click(object? sender, EventArgs e)
        {
            if (dgvTabs.SelectedRows.Count == 0) return;
            int idx = dgvTabs.SelectedRows[0].Index;
            if (idx == 0) return;
            SwapRows(idx, idx - 1);
            dgvTabs.ClearSelection();
            dgvTabs.Rows[idx - 1].Selected = true;
        }

        private void btnMoveDown_Click(object? sender, EventArgs e)
        {
            if (dgvTabs.SelectedRows.Count == 0) return;
            int idx = dgvTabs.SelectedRows[0].Index;
            if (idx == dgvTabs.Rows.Count - 1) return;
            SwapRows(idx, idx + 1);
            dgvTabs.ClearSelection();
            dgvTabs.Rows[idx + 1].Selected = true;
        }

        private void SwapRows(int a, int b)
        {
            for (int col = 0; col < dgvTabs.Columns.Count; col++)
            {
                var temp = dgvTabs.Rows[a].Cells[col].Value;
                dgvTabs.Rows[a].Cells[col].Value = dgvTabs.Rows[b].Cells[col].Value;
                dgvTabs.Rows[b].Cells[col].Value = temp;
            }
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            var endpoint = txtCdpEndpoint.Text.Trim();
            if (string.IsNullOrEmpty(endpoint))
            {
                MessageBox.Show("CDP 엔드포인트를 입력하세요.", "입력 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCdpEndpoint.Focus();
                return;
            }

            var tabs = CollectTabs();
            if (tabs.Count == 0)
            {
                MessageBox.Show("탭 목록에 항목이 하나 이상 있어야 합니다.", "입력 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var updated = new AppConfig
            {
                CdpEndpoint = endpoint,
                AutoConnect = chkAutoConnect.Checked,
                Columns = (int)nudColumns.Value,
                Tabs = tabs,
            };
            AppConfig.Save(updated);

            MessageBox.Show("저장되었습니다.", "알림",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }

        // ── 헬퍼 ─────────────────────────────────────────────────

        private List<TabConfig> CollectTabs()
        {
            var list = new List<TabConfig>();
            foreach (DataGridViewRow row in dgvTabs.Rows)
            {
                var name             = row.Cells[0].Value?.ToString()?.Trim() ?? "";
                var autoRun          = row.Cells[1].Value is true;
                var url              = row.Cells[2].Value?.ToString()?.Trim() ?? "";
                var autoLogin        = row.Cells[3].Value is true;
                var loginId          = row.Cells[4].Value?.ToString()?.Trim() ?? "";
                var loginPwd         = row.Cells[5].Value?.ToString()?.Trim() ?? "";
                var idSelector       = row.Cells[6].Value?.ToString()?.Trim() ?? "";
                var pwdSelector      = row.Cells[7].Value?.ToString()?.Trim() ?? "";
                var submitSelector   = row.Cells[8].Value?.ToString()?.Trim() ?? "";
                var extraSelector    = row.Cells[9].Value?.ToString()?.Trim() ?? "";
                var extraSelectorType = row.Cells[10].Value?.ToString()?.Trim() ?? "text";
                var extraValue       = row.Cells[11].Value?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url))
                    list.Add(new TabConfig
                    {
                        Name = name, AutoRun = autoRun, Url = url,
                        AutoLogin = autoLogin,
                        LoginId        = CredentialHelper.Encrypt(loginId),
                        LoginPwd       = CredentialHelper.Encrypt(loginPwd),
                        IdSelector     = idSelector,
                        PwdSelector    = pwdSelector,
                        SubmitSelector = submitSelector,
                        ExtraInput = new ExtraInput
                        {
                            Selector     = extraSelector,
                            SelectorType = extraSelectorType,
                            Value        = extraValue,
                        },
                    });
            }
            return list;
        }
    }
}
