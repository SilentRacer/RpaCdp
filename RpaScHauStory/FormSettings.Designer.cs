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
            lblColumns = new Label();
            nudColumns = new NumericUpDown();
            lblColumnsUnit = new Label();
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
            colCustomTitle = new DataGridViewTextBoxColumn();
            colTableSelector = new DataGridViewTextBoxColumn();
            colPagingSelector = new DataGridViewTextBoxColumn();
            btnAdd = new Button();
            btnDelete = new Button();
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            chkUserLayout = new CheckBox();
            chkShowPassword = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)nudColumns).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvTabs).BeginInit();
            SuspendLayout();
            // 
            // lblCdp
            // 
            lblCdp.AutoSize = true;
            lblCdp.Enabled = false;
            lblCdp.Location = new Point(1264, 200);
            lblCdp.Name = "lblCdp";
            lblCdp.Size = new Size(193, 32);
            lblCdp.TabIndex = 12;
            lblCdp.Text = "CDP 엔드포인트:";
            // 
            // txtCdpEndpoint
            // 
            txtCdpEndpoint.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCdpEndpoint.Enabled = false;
            txtCdpEndpoint.Location = new Point(1475, 196);
            txtCdpEndpoint.Name = "txtCdpEndpoint";
            txtCdpEndpoint.Size = new Size(500, 39);
            txtCdpEndpoint.TabIndex = 11;
            // 
            // lblColumns
            // 
            lblColumns.AutoSize = true;
            lblColumns.Location = new Point(20, 55);
            lblColumns.Name = "lblColumns";
            lblColumns.Size = new Size(131, 32);
            lblColumns.TabIndex = 9;
            lblColumns.Text = "버튼 열 수:";
            // 
            // nudColumns
            // 
            nudColumns.Location = new Point(155, 52);
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
            lblColumnsUnit.Location = new Point(226, 55);
            lblColumnsUnit.Name = "lblColumnsUnit";
            lblColumnsUnit.Size = new Size(111, 32);
            lblColumnsUnit.TabIndex = 7;
            lblColumnsUnit.Text = "열  (1~5)";
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
            dgvTabs.Columns.AddRange(new DataGridViewColumn[] { colName, colAutoRun, colUrl, colCustomTitle, colAutoLogin, colLoginId, colLoginPwd, colIdSelector, colPwdSelector, colSubmitSelector, colExtraSelector, colExtraSelectorType, colExtraValue, colTableSelector, colPagingSelector });
            dgvTabs.Location = new Point(20, 152);
            dgvTabs.MultiSelect = false;
            dgvTabs.Name = "dgvTabs";
            dgvTabs.RowHeadersVisible = false;
            dgvTabs.RowHeadersWidth = 82;
            dgvTabs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTabs.Size = new Size(1955, 468);
            dgvTabs.TabIndex = 0;
            dgvTabs.CellContentClick += dgvTabs_CellContentClick;
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
            colAutoRun.HeaderText = "자동\n실행";
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
            colAutoLogin.HeaderText = "자동\n로그온";
            colAutoLogin.MinimumWidth = 10;
            colAutoLogin.Name = "colAutoLogin";
            colAutoLogin.ToolTipText = "탭 열기 후 자동으로 로그온할지 여부";
            colAutoLogin.Width = 90;
            // 
            // colLoginId
            // 
            colLoginId.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            colLoginId.HeaderText = "아이디";
            colLoginId.MinimumWidth = 100;
            colLoginId.Name = "colLoginId";
            // 
            // colLoginPwd
            // 
            colLoginPwd.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            colLoginPwd.HeaderText = "비밀번호";
            colLoginPwd.MinimumWidth = 100;
            colLoginPwd.Name = "colLoginPwd";
            // 
            // colIdSelector
            // 
            colIdSelector.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            colIdSelector.HeaderText = "ID\n셀렉터";
            colIdSelector.MinimumWidth = 100;
            colIdSelector.Name = "colIdSelector";
            colIdSelector.ToolTipText = "아이디 입력 필드 CSS 셀렉터 (비워 두면 자동 탐지)";
            // 
            // colPwdSelector
            // 
            colPwdSelector.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            colPwdSelector.HeaderText = "비밀번호\n셀렉터";
            colPwdSelector.MinimumWidth = 100;
            colPwdSelector.Name = "colPwdSelector";
            colPwdSelector.ToolTipText = "비밀번호 입력 필드 CSS 셀렉터 (비워 두면 자동 탐지)";
            // 
            // colSubmitSelector
            // 
            colSubmitSelector.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colSubmitSelector.HeaderText = "로그인\n셀렉터";
            colSubmitSelector.MinimumWidth = 110;
            colSubmitSelector.Name = "colSubmitSelector";
            colSubmitSelector.ToolTipText = "로그온 버튼 CSS 셀렉터 (비워 두면 자동 탐지)";
            colSubmitSelector.Width = 131;
            // 
            // colExtraSelector
            // 
            colExtraSelector.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colExtraSelector.HeaderText = "추가 셀렉터";
            colExtraSelector.MinimumWidth = 10;
            colExtraSelector.Name = "colExtraSelector";
            colExtraSelector.ToolTipText = "로그온 전 추가 입력 필드 CSS 셀렉터";
            colExtraSelector.Width = 151;
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
            colExtraSelectorType.Width = 112;
            // 
            // colExtraValue
            // 
            colExtraValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            colExtraValue.HeaderText = "추가 내용";
            colExtraValue.MinimumWidth = 300;
            colExtraValue.Name = "colExtraValue";
            colExtraValue.ToolTipText = "입력할 텍스트 또는 선택할 옵션 표시명";
            colExtraValue.Width = 300;
            //
            // colCustomTitle
            //
            colCustomTitle.HeaderText = "탭 제목\n(앞에 추가)";
            colCustomTitle.MinimumWidth = 100;
            colCustomTitle.Name = "colCustomTitle";
            colCustomTitle.ToolTipText = "페이지 제목이 중복될 때 제목앞에　추가. 탭 전환 감지에 사용됩니다. (비워 두면 실제 페이지 제목 사용)";
            colCustomTitle.Width = 200;
            //
            // colTableSelector
            //
            colTableSelector.HeaderText = "테이블\n셀렉터";
            colTableSelector.MinimumWidth = 100;
            colTableSelector.Name = "colTableSelector";
            colTableSelector.ToolTipText = "데이터 테이블 CSS 셀렉터 (비워 두면 행이 가장 많은 테이블 자동 선택)";
            colTableSelector.Width = 200;
            //
            // colPagingSelector
            //
            colPagingSelector.HeaderText = "페이징\n셀렉터";
            colPagingSelector.MinimumWidth = 100;
            colPagingSelector.Name = "colPagingSelector";
            colPagingSelector.ToolTipText = "페이징 컨테이너 CSS 셀렉터 (비워 두면 id에 Paging 포함된 요소 자동 탐지)";
            colPagingSelector.Width = 200;
            //
            // btnAdd
            //
            btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAdd.Location = new Point(24, 641);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(111, 38);
            btnAdd.TabIndex = 5;
            btnAdd.Text = "+ 추가";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(144, 641);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(111, 38);
            btnDelete.TabIndex = 4;
            btnDelete.Text = "- 삭제";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnMoveUp
            // 
            btnMoveUp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnMoveUp.Location = new Point(264, 641);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(111, 38);
            btnMoveUp.TabIndex = 3;
            btnMoveUp.Text = "▲ 위로";
            btnMoveUp.UseVisualStyleBackColor = true;
            // 
            // btnMoveDown
            // 
            btnMoveDown.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnMoveDown.Location = new Point(384, 641);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(111, 38);
            btnMoveDown.TabIndex = 2;
            btnMoveDown.Text = "▼ 아래로";
            btnMoveDown.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(1800, 641);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(85, 38);
            btnSave.TabIndex = 1;
            btnSave.Text = "저장";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(1895, 641);
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
            chkUserLayout.Location = new Point(543, 641);
            chkUserLayout.Name = "chkUserLayout";
            chkUserLayout.Size = new Size(278, 36);
            chkUserLayout.TabIndex = 0;
            chkUserLayout.Text = "사용자 레이아웃 기억";
            chkUserLayout.UseVisualStyleBackColor = true;
            // 
            // chkShowPassword
            // 
            chkShowPassword.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            chkShowPassword.AutoSize = true;
            chkShowPassword.Location = new Point(1179, 55);
            chkShowPassword.Name = "chkShowPassword";
            chkShowPassword.Size = new Size(184, 36);
            chkShowPassword.TabIndex = 0;
            chkShowPassword.Text = "ID,PWD 편집";
            chkShowPassword.UseVisualStyleBackColor = true;
            // 
            // FormSettings
            // 
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1995, 768);
            Controls.Add(chkShowPassword);
            Controls.Add(chkUserLayout);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(btnMoveDown);
            Controls.Add(btnMoveUp);
            Controls.Add(btnDelete);
            Controls.Add(btnAdd);
            Controls.Add(dgvTabs);
            Controls.Add(lblColumnsUnit);
            Controls.Add(nudColumns);
            Controls.Add(lblColumns);
            Controls.Add(txtCdpEndpoint);
            Controls.Add(lblCdp);
            MinimumSize = new Size(500, 440);
            Name = "FormSettings";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "환경 설정";
            ((System.ComponentModel.ISupportInitialize)nudColumns).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvTabs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCdp;
        private TextBox txtCdpEndpoint;
        private Label lblColumns;
        private NumericUpDown nudColumns;
        private Label lblColumnsUnit;
        private DataGridView dgvTabs;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnSave;
        private Button btnCancel;
        private CheckBox chkUserLayout;
        private CheckBox chkShowPassword;
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
        private DataGridViewTextBoxColumn colCustomTitle;
        private DataGridViewTextBoxColumn colTableSelector;
        private DataGridViewTextBoxColumn colPagingSelector;
    }
}
