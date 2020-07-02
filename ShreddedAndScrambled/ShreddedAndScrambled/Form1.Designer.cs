namespace ShreddedAndScrambled {
    partial class Form1 {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.btnRun = new System.Windows.Forms.Button();
            this.tabLayPnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.txtBoxLog = new System.Windows.Forms.TextBox();
            this.PicBoxMaster = new System.Windows.Forms.PictureBox();
            this.tabLayPnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicBoxMaster)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tabLayPnlMain.SetColumnSpan(this.btnRun, 2);
            this.btnRun.Location = new System.Drawing.Point(554, 834);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "&Run!";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.Run_Click);
            // 
            // tabLayPnlMain
            // 
            this.tabLayPnlMain.ColumnCount = 2;
            this.tabLayPnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabLayPnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tabLayPnlMain.Controls.Add(this.btnRun, 0, 3);
            this.tabLayPnlMain.Controls.Add(this.txtBoxLog, 0, 2);
            this.tabLayPnlMain.Controls.Add(this.PicBoxMaster, 0, 0);
            this.tabLayPnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLayPnlMain.Location = new System.Drawing.Point(0, 0);
            this.tabLayPnlMain.Name = "tabLayPnlMain";
            this.tabLayPnlMain.RowCount = 4;
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tabLayPnlMain.Size = new System.Drawing.Size(1184, 861);
            this.tabLayPnlMain.TabIndex = 1;
            // 
            // txtBoxLog
            // 
            this.tabLayPnlMain.SetColumnSpan(this.txtBoxLog, 2);
            this.txtBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBoxLog.Location = new System.Drawing.Point(3, 603);
            this.txtBoxLog.Multiline = true;
            this.txtBoxLog.Name = "txtBoxLog";
            this.txtBoxLog.ReadOnly = true;
            this.txtBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxLog.Size = new System.Drawing.Size(1178, 225);
            this.txtBoxLog.TabIndex = 1;
            // 
            // PicBoxMaster
            // 
            this.PicBoxMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicBoxMaster.Location = new System.Drawing.Point(3, 3);
            this.PicBoxMaster.Name = "PicBoxMaster";
            this.tabLayPnlMain.SetRowSpan(this.PicBoxMaster, 2);
            this.PicBoxMaster.Size = new System.Drawing.Size(878, 594);
            this.PicBoxMaster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PicBoxMaster.TabIndex = 2;
            this.PicBoxMaster.TabStop = false;
            this.PicBoxMaster.Click += new System.EventHandler(this.PicBoxMaster_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 861);
            this.Controls.Add(this.tabLayPnlMain);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shredded & Scrambled";
            this.tabLayPnlMain.ResumeLayout(false);
            this.tabLayPnlMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicBoxMaster)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.TableLayoutPanel tabLayPnlMain;
        private System.Windows.Forms.TextBox txtBoxLog;
        private System.Windows.Forms.PictureBox PicBoxMaster;
    }
}

