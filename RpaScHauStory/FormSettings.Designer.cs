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
            lblCdp = new Label();
            txtCdpEndpoint = new TextBox();
            lblTabs = new Label();
            dgvTabs = new DataGridView();
            colName = new DataGridViewTextBoxColumn();
            colUrl = new DataGridViewTextBoxColumn();
            btnAdd = new Button();
            btnDelete = new Button();
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvTabs).BeginInit();
            SuspendLayout();
            //
            // lblCdp
            //
            lblCdp.AutoSize = true;
            lblCdp.Location = new Point(20, 22);
            lblCdp.Name = "lblCdp";
            lblCdp.Text = "CDP 엔드포인트:";
            //
            // txtCdpEndpoint
            //
            txtCdpEndpoint.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCdpEndpoint.Location = new Point(170, 18);
            txtCdpEndpoint.Name = "txtCdpEndpoint";
            txtCdpEndpoint.Size = new Size(390, 32);
            //
            // lblTabs
            //
            lblTabs.AutoSize = true;
            lblTabs.Location = new Point(20, 68);
            lblTabs.Name = "lblTabs";
            lblTabs.Text = "탭 목록  (버튼 이름과 URL을 직접 수정할 수 있습니다)";
            //
            // dgvTabs
            //
            dgvTabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvTabs.AllowUserToAddRows = false;
            dgvTabs.AllowUserToDeleteRows = false;
            dgvTabs.AllowUserToResizeRows = false;
            dgvTabs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTabs.Columns.AddRange(new DataGridViewColumn[] { colName, colUrl });
            dgvTabs.Location = new Point(20, 95);
            dgvTabs.MultiSelect = false;
            dgvTabs.Name = "dgvTabs";
            dgvTabs.RowHeadersVisible = false;
            dgvTabs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTabs.Size = new Size(560, 250);
            dgvTabs.TabIndex = 0;
            //
            // colName
            //
            colName.HeaderText = "버튼 이름";
            colName.Name = "colName";
            colName.Width = 140;
            //
            // colUrl
            //
            colUrl.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colUrl.HeaderText = "URL";
            colUrl.Name = "colUrl";
            //
            // btnAdd
            //
            btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAdd.Location = new Point(20, 360);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(85, 38);
            btnAdd.Text = "+ 추가";
            btnAdd.UseVisualStyleBackColor = true;
            //
            // btnDelete
            //
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(115, 360);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(85, 38);
            btnDelete.Text = "- 삭제";
            btnDelete.UseVisualStyleBackColor = true;
            //
            // btnMoveUp
            //
            btnMoveUp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnMoveUp.Location = new Point(210, 360);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(85, 38);
            btnMoveUp.Text = "▲ 위로";
            btnMoveUp.UseVisualStyleBackColor = true;
            //
            // btnMoveDown
            //
            btnMoveDown.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnMoveDown.Location = new Point(305, 360);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(85, 38);
            btnMoveDown.Text = "▼ 아래로";
            btnMoveDown.UseVisualStyleBackColor = true;
            //
            // btnSave
            //
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(405, 360);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(85, 38);
            btnSave.Text = "저장";
            btnSave.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(500, 360);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(85, 38);
            btnCancel.Text = "취소";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // FormSettings
            //
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 420);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(btnMoveDown);
            Controls.Add(btnMoveUp);
            Controls.Add(btnDelete);
            Controls.Add(btnAdd);
            Controls.Add(dgvTabs);
            Controls.Add(lblTabs);
            Controls.Add(txtCdpEndpoint);
            Controls.Add(lblCdp);
            MinimumSize = new Size(500, 400);
            Name = "FormSettings";
            StartPosition = FormStartPosition.CenterParent;
            Text = "환경 설정";
            ((System.ComponentModel.ISupportInitialize)dgvTabs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCdp;
        private TextBox txtCdpEndpoint;
        private Label lblTabs;
        private DataGridView dgvTabs;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colUrl;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnSave;
        private Button btnCancel;
    }
}
