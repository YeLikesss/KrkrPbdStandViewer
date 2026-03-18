namespace PbdTJSConverter
{
    partial class MainForm
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
            System.Windows.Forms.Label lbGames;
            System.Windows.Forms.Label lbParams;
            System.Windows.Forms.Label lbLog;
            System.Windows.Forms.Button btnConvert;
            cbTitles = new System.Windows.Forms.ComboBox();
            tbParams = new System.Windows.Forms.TextBox();
            tbLog = new System.Windows.Forms.TextBox();
            lbGames = new System.Windows.Forms.Label();
            lbParams = new System.Windows.Forms.Label();
            lbLog = new System.Windows.Forms.Label();
            btnConvert = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lbGames
            // 
            lbGames.AutoSize = true;
            lbGames.Location = new System.Drawing.Point(12, 15);
            lbGames.Name = "lbGames";
            lbGames.Size = new System.Drawing.Size(32, 17);
            lbGames.TabIndex = 0;
            lbGames.Text = "游戏";
            lbGames.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbGames.UseMnemonic = false;
            // 
            // lbParams
            // 
            lbParams.AutoSize = true;
            lbParams.Location = new System.Drawing.Point(12, 49);
            lbParams.Name = "lbParams";
            lbParams.Size = new System.Drawing.Size(32, 17);
            lbParams.TabIndex = 1;
            lbParams.Text = "参数";
            lbParams.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbParams.UseMnemonic = false;
            // 
            // lbLog
            // 
            lbLog.AutoSize = true;
            lbLog.Location = new System.Drawing.Point(12, 196);
            lbLog.Name = "lbLog";
            lbLog.Size = new System.Drawing.Size(32, 17);
            lbLog.TabIndex = 5;
            lbLog.Text = "日志";
            lbLog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbLog.UseMnemonic = false;
            // 
            // btnConvert
            // 
            btnConvert.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnConvert.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            btnConvert.Location = new System.Drawing.Point(674, 362);
            btnConvert.Name = "btnConvert";
            btnConvert.Size = new System.Drawing.Size(98, 38);
            btnConvert.TabIndex = 7;
            btnConvert.Text = "转换";
            btnConvert.UseMnemonic = false;
            btnConvert.UseVisualStyleBackColor = true;
            btnConvert.Click += BtnConvert_OnClick;
            // 
            // cbTitles
            // 
            cbTitles.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbTitles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbTitles.IntegralHeight = false;
            cbTitles.Location = new System.Drawing.Point(50, 12);
            cbTitles.MaxDropDownItems = 16;
            cbTitles.Name = "cbTitles";
            cbTitles.Size = new System.Drawing.Size(722, 25);
            cbTitles.TabIndex = 2;
            cbTitles.SelectedIndexChanged += CbTitles_OnSelectedIndexChanged;
            // 
            // tbParams
            // 
            tbParams.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbParams.Location = new System.Drawing.Point(50, 46);
            tbParams.Multiline = true;
            tbParams.Name = "tbParams";
            tbParams.ReadOnly = true;
            tbParams.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            tbParams.Size = new System.Drawing.Size(722, 142);
            tbParams.TabIndex = 4;
            tbParams.WordWrap = false;
            // 
            // tbLog
            // 
            tbLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbLog.Location = new System.Drawing.Point(50, 196);
            tbLog.MaxLength = 65536;
            tbLog.Multiline = true;
            tbLog.Name = "tbLog";
            tbLog.ReadOnly = true;
            tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            tbLog.Size = new System.Drawing.Size(722, 160);
            tbLog.TabIndex = 6;
            tbLog.WordWrap = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            ClientSize = new System.Drawing.Size(784, 412);
            Controls.Add(btnConvert);
            Controls.Add(tbLog);
            Controls.Add(lbLog);
            Controls.Add(tbParams);
            Controls.Add(cbTitles);
            Controls.Add(lbParams);
            Controls.Add(lbGames);
            DoubleBuffered = true;
            ImeMode = System.Windows.Forms.ImeMode.Disable;
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "立绘TJS对象转换";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.ComboBox cbTitles;
        private System.Windows.Forms.TextBox tbParams;
        private System.Windows.Forms.TextBox tbLog;
    }
}