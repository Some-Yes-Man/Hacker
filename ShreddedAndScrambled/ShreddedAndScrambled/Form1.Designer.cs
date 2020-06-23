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
            this.tabLayPnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tabLayPnlMain.SetColumnSpan(this.btnRun, 2);
            this.btnRun.Location = new System.Drawing.Point(362, 423);
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
            this.tabLayPnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tabLayPnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tabLayPnlMain.Controls.Add(this.btnRun, 0, 1);
            this.tabLayPnlMain.Controls.Add(this.txtBoxLog, 0, 0);
            this.tabLayPnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLayPnlMain.Location = new System.Drawing.Point(0, 0);
            this.tabLayPnlMain.Name = "tabLayPnlMain";
            this.tabLayPnlMain.RowCount = 2;
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tabLayPnlMain.Size = new System.Drawing.Size(800, 450);
            this.tabLayPnlMain.TabIndex = 1;
            // 
            // txtBoxLog
            // 
            this.tabLayPnlMain.SetColumnSpan(this.txtBoxLog, 2);
            this.txtBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBoxLog.Location = new System.Drawing.Point(3, 3);
            this.txtBoxLog.Multiline = true;
            this.txtBoxLog.Name = "txtBoxLog";
            this.txtBoxLog.ReadOnly = true;
            this.txtBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxLog.Size = new System.Drawing.Size(794, 414);
            this.txtBoxLog.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabLayPnlMain);
            this.Name = "Form1";
            this.Text = "Shredded & Scrambled";
            this.tabLayPnlMain.ResumeLayout(false);
            this.tabLayPnlMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.TableLayoutPanel tabLayPnlMain;
        private System.Windows.Forms.TextBox txtBoxLog;
    }
}

