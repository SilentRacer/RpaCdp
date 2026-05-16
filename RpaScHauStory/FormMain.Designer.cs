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
            btnConnect = new Button();
            btnSettings = new Button();
            pnlTabs = new FlowLayoutPanel();
            lblStatus = new Label();
            SuspendLayout();
            //
            // btnConnect
            //
            btnConnect.Location = new Point(20, 20);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(150, 40);
            btnConnect.TabIndex = 0;
            btnConnect.Text = "브라우저 연결";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            //
            // btnSettings
            //
            btnSettings.Location = new Point(180, 20);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(80, 40);
            btnSettings.TabIndex = 1;
            btnSettings.Text = "설정";
            btnSettings.UseVisualStyleBackColor = true;
            btnSettings.Click += btnSettings_Click;
            //
            // pnlTabs
            //
            pnlTabs.FlowDirection = FlowDirection.LeftToRight;
            pnlTabs.Location = new Point(20, 78);
            pnlTabs.Name = "pnlTabs";
            pnlTabs.Size = new Size(240, 0);
            pnlTabs.TabIndex = 2;
            pnlTabs.WrapContents = true;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(20, 94);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 3;
            lblStatus.Text = "대기 중...";
            //
            // FormMain
            //
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(280, 200);
            Controls.Add(lblStatus);
            Controls.Add(pnlTabs);
            Controls.Add(btnSettings);
            Controls.Add(btnConnect);
            Name = "FormMain";
            Text = "RPA - 탭 관리";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnConnect;
        private Button btnSettings;
        private FlowLayoutPanel pnlTabs;
        private Label lblStatus;
    }
}
