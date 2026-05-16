namespace RpaScHauStory
{
    partial class FormSettings
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            lblCdp = new Label();
            txtCdpEndpoint = new TextBox();
            chkAutoConnect = new CheckBox();
            lblColumns = new Label();
            nudColumns = new NumericUpDown();
            lblColumnsUnit = new Label();
            lblTabs = new Label();
            dgvTabs = new DataGridView();
            colName = new DataGridViewTextBoxColumn();
            colAutoRun = new DataGridViewCheckBoxColumn();
            colUrl = new DataGridViewTextBoxColumn();
            colAutoLogin = new DataGridViewCheckBoxColumn();
            colLoginId = new DataGridViewTextBoxColumn();
            colLoginPwd = new DataGridViewTextBoxColumn();
            colIdSelector = new DataGridViewTextBoxColumn();
            colPwdSelector = new DataGridViewTextBoxColumn();
            colSubmitSelector = new DataGridViewTextBoxColumn();
            colExtraSelector = new DataGridViewTextBoxColumn();
            colExtraSelectorType = new DataGridViewComboBoxColumn();
            colExtraValue = new DataGridViewTextBoxColumn();
            btnAdd = new Button();
            btnDelete = new Button();
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            chkUserLayout = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)nudColumns).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvTabs).BeginInit();
            SuspendLayout();
            // 
            // lblCdp
            // 
            lblCdp.AutoSize = true;
            lblCdp.Location = new Point(20, 22);
            lblCdp.Name = "lblCdp";
            lblCdp.Size = new Size(193, 32);
            lblCdp.TabIndex = 12;
            lblCdp.Text = "CDP 엔드포인트:";
            // 
            // txtCdpEndpoint
            // 
            txtCdpEndpoint.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCdpEndpoint.Location = new Point(231, 18);
            txtCdpEndpoint.Name = "txtCdpEndpoint";
            txtCdpEndpoint.Size = new Size(3084, 39);
            txtCdpEndpoint.TabIndex = 11;
            // 
            // chkAutoConnect
            // 
            chkAutoConnect.AutoSize = true;
            chkAutoConnect.Location = new Point(20, 64);
            chkAutoConnect.Name = "chkAutoConnect";
            chkAutoConnect.Size = new Size(374, 36);
            chkAutoConnect.TabIndex = 10;
            chkAutoConnect.Text = "앱 시작 시 브라우저 자동 연결";
            // 
            // lblColumns
            // 
            lblColumns.AutoSize = true;
            lblColumns.Location = new Point(20, 106);
            lblColumns.Name = "lblColumns";
            lblColumns.Size = new Size(131, 32);
            lblColumns.TabIndex = 9;
            lblColumns.Text = "버튼 열 수:";
            // 
            // nudColumns
            // 
            nudColumns.Location = new Point(155, 103);
            nudColumns.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            nudColumns.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudColumns.Name = "nudColumns";
            nudColumns.Size = new Size(60, 39);
            nudColumns.TabIndex = 8;
            nudColumns.TextAlign = HorizontalAlignment.Center;
            nudColumns.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblColumnsUnit
            // 
            lblColumnsUnit.AutoSize = true;
            lblColumnsUnit.Location = new Point(226, 106);
            lblColumnsUnit.Name = "lblColumnsUnit";
            lblColumnsUnit.Size = new Size(111, 32);
            lblColumnsUnit.TabIndex = 7;
            lblColumnsUnit.Text = "열  (1~5)";
            // 
            // lblTabs
            // 
            lblTabs.AutoSize = true;
            lblTabs.Location = new Point(20, 152);
            lblTabs.Name = "lblTabs";
            lblTabs.Size = new Size(600, 32);
            lblTabs.TabIndex = 6;
            lblTabs.Text = "탭 목록  (버튼 이름과 URL을 직접 수정할 수 있습니다)";
            // 
            // dgvTabs
            // 
            dgvTabs.AllowUserToAddRows = false;
            dgvTabs.AllowUserToDeleteRows = false;
            dgvTabs.AllowUserToResizeRows = false;
            dgvTabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("맑은 고딕", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvTabs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvTabs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTabs.Columns.AddRange(new DataGridViewColumn[] { colName, colAutoRun, colUrl, colAutoLogin, colLoginId, colLoginPwd, colIdSelector, colPwdSelector, colSubmitSelector, colExtraSelector, colExtraSelectorType, colExtraValue });
            dgvTabs.Location = new Point(20, 199);
            dgvTabs.MultiSelect = false;
            dgvTabs.Name = "dgvTabs";
            dgvTabs.RowHeadersVisible = false;
            dgvTabs.RowHeadersWidth = 82;
            dgvTabs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTabs.Size = new Size(1955, 513);
            dgvTabs.TabIndex = 0;
            // 
            // colName
            // 
            colName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            colName.HeaderText = "버튼 이름";
            colName.MinimumWidth = 10;
            colName.Name = "colName";
            colName.Width = 10;
            // 
            // colAutoRun
            // 
            colAutoRun.HeaderText = "자동 실행";
            colAutoRun.MinimumWidth = 10;
            colAutoRun.Name = "colAutoRun";
            colAutoRun.ToolTipText = "자동 연결 후 이 탭을 자동으로 열지 여부";
            colAutoRun.Width = 80;
            // 
            // colUrl
            // 
            colUrl.HeaderText = "URL";
            colUrl.MinimumWidth = 120;
            colUrl.Name = "colUrl";
            colUrl.Width = 300;
            // 
            // colAutoLogin
            // 
            colAutoLogin.HeaderText = "자동 로그온";
            colAutoLogin.MinimumWidth = 10;
            colAutoLogin.Name = "colAutoLogin";
            colAutoLogin.ToolTipText = "탭 열기 후 자동으로 로그온할지 여부";
            colAutoLogin.Width = 90;
            // 
            // colLoginId
            // 
            colLoginId.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            colLoginId.HeaderText = "아이디";
            colLoginId.MinimumWidth = 10;
            colLoginId.Name = "colLoginId";
            colLoginId.Width = 10;
            // 
            // colLoginPwd
            // 
            colLoginPwd.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            colLoginPwd.HeaderText = "비밀번호";
            colLoginPwd.MinimumWidth = 10;
            colLoginPwd.Name = "colLoginPwd";
            colLoginPwd.Width = 10;
            // 
            // colIdSelector
            // 
            colIdSelector.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            colIdSelector.HeaderText = "ID 셀렉터";
            colIdSelector.MinimumWidth = 10;
            colIdSelector.Name = "colIdSelector";
            colIdSelector.ToolTipText = "아이디 입력 필드 CSS 셀렉터 (비워 두면 자동 탐지)";
            colIdSelector.Width = 10;
            // 
            // colPwdSelector
            // 
            colPwdSelector.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            colPwdSelector.HeaderText = "PWD 셀렉터";
            colPwdSelector.MinimumWidth = 10;
            colPwdSelector.Name = "colPwdSelector";
            colPwdSelector.ToolTipText = "비밀번호 입력 필드 CSS 셀렉터 (비워 두면 자동 탐지)";
            colPwdSelector.Width = 10;
            // 
            // colSubmitSelector
            // 
            colSubmitSelector.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colSubmitSelector.HeaderText = "로그인 셀렉터";
            colSubmitSelector.MinimumWidth = 10;
            colSubmitSelector.Name = "colSubmitSelector";
            colSubmitSelector.ToolTipText = "로그온 버튼 CSS 셀렉터 (비워 두면 자동 탐지)";
            colSubmitSelector.Width = 211;
            // 
            // colExtraSelector
            // 
            colExtraSelector.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colExtraSelector.HeaderText = "추가 셀렉터";
            colExtraSelector.MinimumWidth = 10;
            colExtraSelector.Name = "colExtraSelector";
            colExtraSelector.ToolTipText = "로그온 전 추가 입력 필드 CSS 셀렉터";
            colExtraSelector.Width = 187;
            // 
            // colExtraSelectorType
            // 
            colExtraSelectorType.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewCellStyle2.NullValue = "text";
            colExtraSelectorType.DefaultCellStyle = dataGridViewCellStyle2;
            colExtraSelectorType.HeaderText = "셀렉터 종류";
            colExtraSelectorType.Items.AddRange(new object[] { "text", "select" });
            colExtraSelectorType.MinimumWidth = 10;
            colExtraSelectorType.Name = "colExtraSelectorType";
            colExtraSelectorType.ToolTipText = "text: 텍스트 입력 / select: 드롭다운 표시명 선택";
            colExtraSelectorType.Width = 148;
            // 
            // colExtraValue
            // 
            colExtraValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            colExtraValue.HeaderText = "추가 입력값";
            colExtraValue.MinimumWidth = 200;
            colExtraValue.Name = "colExtraValue";
            colExtraValue.ToolTipText = "입력할 텍스트 또는 선택할 옵션 표시명";
            colExtraValue.Width = 200;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAdd.Location = new Point(20, 724);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(85, 38);
            btnAdd.TabIndex = 5;
            btnAdd.Text = "+ 추가";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(115, 724);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(85, 38);
            btnDelete.TabIndex = 4;
            btnDelete.Text = "- 삭제";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnMoveUp
            // 
            btnMoveUp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnMoveUp.Location = new Point(210, 724);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(85, 38);
            btnMoveUp.TabIndex = 3;
            btnMoveUp.Text = "▲ 위로";
            btnMoveUp.UseVisualStyleBackColor = true;
            // 
            // btnMoveDown
            // 
            btnMoveDown.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnMoveDown.Location = new Point(305, 724);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(85, 38);
            btnMoveDown.TabIndex = 2;
            btnMoveDown.Text = "▼ 아래로";
            btnMoveDown.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(1800, 724);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(85, 38);
            btnSave.TabIndex = 1;
            btnSave.Text = "저장";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(1895, 724);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(85, 38);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "취소";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // chkUserLayout
            //
            chkUserLayout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            chkUserLayout.AutoSize = true;
            chkUserLayout.Location = new Point(410, 730);
            chkUserLayout.Name = "chkUserLayout";
            chkUserLayout.Text = "사용자 레이아웃 기억";
            chkUserLayout.UseVisualStyleBackColor = true;
            //
            // FormSettings
            // 
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1995, 768);
            Controls.Add(chkUserLayout);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(btnMoveDown);
            Controls.Add(btnMoveUp);
            Controls.Add(btnDelete);
            Controls.Add(btnAdd);
            Controls.Add(dgvTabs);
            Controls.Add(lblTabs);
            Controls.Add(lblColumnsUnit);
            Controls.Add(nudColumns);
            Controls.Add(lblColumns);
            Controls.Add(chkAutoConnect);
            Controls.Add(txtCdpEndpoint);
            Controls.Add(lblCdp);
            MinimumSize = new Size(500, 440);
            Name = "FormSettings";
            StartPosition = FormStartPosition.CenterParent;
            Text = "환경 설정";
            ((System.ComponentModel.ISupportInitialize)nudColumns).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvTabs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCdp;
        private TextBox txtCdpEndpoint;
        private CheckBox chkAutoConnect;
        private Label lblColumns;
        private NumericUpDown nudColumns;
        private Label lblColumnsUnit;
        private Label lblTabs;
        private DataGridView dgvTabs;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnSave;
        private Button btnCancel;
        private CheckBox chkUserLayout;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewCheckBoxColumn colAutoRun;
        private DataGridViewTextBoxColumn colUrl;
        private DataGridViewCheckBoxColumn colAutoLogin;
        private DataGridViewTextBoxColumn colLoginId;
        private DataGridViewTextBoxColumn colLoginPwd;
        private DataGridViewTextBoxColumn colIdSelector;
        private DataGridViewTextBoxColumn colPwdSelector;
        private DataGridViewTextBoxColumn colSubmitSelector;
        private DataGridViewTextBoxColumn colExtraSelector;
        private DataGridViewComboBoxColumn colExtraSelectorType;
        private DataGridViewTextBoxColumn colExtraValue;
    }
}
