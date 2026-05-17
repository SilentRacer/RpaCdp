namespace RpaScHauStory
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlTop = new Panel();
            btnConnect = new Button();
            btnSettings = new Button();
            pnlMiddle = new Panel();
            pnlTabs = new FlowLayoutPanel();
            pnlBottom = new Panel();
            btnSaveExcel = new Button();
            lblStatus = new Label();
            pnlTop.SuspendLayout();
            pnlMiddle.SuspendLayout();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(btnConnect);
            pnlTop.Controls.Add(btnSettings);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1038, 76);
            pnlTop.TabIndex = 2;
            // 
            // btnConnect
            // 
            btnConnect.BackColor = SystemColors.ControlLight;
            btnConnect.Location = new Point(20, 15);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(160, 46);
            btnConnect.TabIndex = 0;
            btnConnect.Text = "연결 확인";
            btnConnect.UseVisualStyleBackColor = false;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = SystemColors.ControlLight;
            btnSettings.Location = new Point(192, 15);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(160, 46);
            btnSettings.TabIndex = 1;
            btnSettings.Text = "환경 설정";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // pnlMiddle
            // 
            pnlMiddle.AutoScroll = true;
            pnlMiddle.Controls.Add(pnlTabs);
            pnlMiddle.Dock = DockStyle.Fill;
            pnlMiddle.Location = new Point(0, 76);
            pnlMiddle.Name = "pnlMiddle";
            pnlMiddle.Padding = new Padding(10);
            pnlMiddle.Size = new Size(1038, 269);
            pnlMiddle.TabIndex = 0;
            // 
            // pnlTabs
            // 
            pnlTabs.Location = new Point(10, 10);
            pnlTabs.Name = "pnlTabs";
            pnlTabs.Size = new Size(200, 0);
            pnlTabs.TabIndex = 0;
            // 
            // pnlBottom
            // 
            pnlBottom.Controls.Add(btnSaveExcel);
            pnlBottom.Controls.Add(lblStatus);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 345);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1038, 146);
            pnlBottom.TabIndex = 1;
            // 
            // btnSaveExcel
            // 
            btnSaveExcel.BackColor = Color.FromArgb(255, 192, 128);
            btnSaveExcel.Location = new Point(30, 21);
            btnSaveExcel.Name = "btnSaveExcel";
            btnSaveExcel.Size = new Size(160, 46);
            btnSaveExcel.TabIndex = 2;
            btnSaveExcel.Text = "EXCEL 저장";
            btnSaveExcel.UseVisualStyleBackColor = false;
            btnSaveExcel.Click += btnSaveExcel_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(39, 88);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(109, 32);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "대기 중...";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1038, 491);
            Controls.Add(pnlMiddle);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            MinimumSize = new Size(400, 300);
            Name = "FormMain";
            Text = "RPA - 탭 관리";
            Load += FormMain_Load_1;
            pnlTop.ResumeLayout(false);
            pnlMiddle.ResumeLayout(false);
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Panel pnlMiddle;
        private Panel pnlBottom;
        private Button btnConnect;
        private Button btnSettings;
        private FlowLayoutPanel pnlTabs;
        private Label lblStatus;
        private Button btnSaveExcel;
    }
}
