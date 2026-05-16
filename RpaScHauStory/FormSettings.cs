namespace RpaScHauStory
{
    public partial class FormSettings : Form
    {
        private readonly AppConfig _config;

        public FormSettings(AppConfig config)
        {
            InitializeComponent();
            _config = config;

            Load += FormSettings_Load;
            btnAdd.Click += btnAdd_Click;
            btnDelete.Click += btnDelete_Click;
            btnMoveUp.Click += btnMoveUp_Click;
            btnMoveDown.Click += btnMoveDown_Click;
            btnSave.Click += btnSave_Click;
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        }

        // ── 초기화 ────────────────────────────────────────────────

        private void FormSettings_Load(object? sender, EventArgs e)
        {
            txtCdpEndpoint.Text = _config.CdpEndpoint;

            foreach (var tab in _config.Tabs)
                dgvTabs.Rows.Add(tab.Name, tab.Url);
        }

        // ── 버튼 핸들러 ───────────────────────────────────────────

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            int idx = dgvTabs.Rows.Add("새 버튼", "https://");
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

            var updated = new AppConfig { CdpEndpoint = endpoint, Tabs = tabs };
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
                var name = row.Cells[0].Value?.ToString()?.Trim() ?? "";
                var url  = row.Cells[1].Value?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url))
                    list.Add(new TabConfig { Name = name, Url = url });
            }
            return list;
        }
    }
}
