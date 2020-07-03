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
            this.GrpBoxSelection = new System.Windows.Forms.GroupBox();
            this.PicBoxSelection = new System.Windows.Forms.PictureBox();
            this.GrpBoxMain = new System.Windows.Forms.GroupBox();
            this.PicBoxMaster = new System.Windows.Forms.PictureBox();
            this.ListViewPieceSelection = new System.Windows.Forms.ListView();
            this.ListViewColOne = new System.Windows.Forms.ColumnHeader();
            this.tabLayPnlMain.SuspendLayout();
            this.GrpBoxSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicBoxSelection)).BeginInit();
            this.GrpBoxMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicBoxMaster)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tabLayPnlMain.SetColumnSpan(this.btnRun, 2);
            this.btnRun.Location = new System.Drawing.Point(554, 934);
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
            this.tabLayPnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F));
            this.tabLayPnlMain.Controls.Add(this.btnRun, 0, 3);
            this.tabLayPnlMain.Controls.Add(this.txtBoxLog, 0, 2);
            this.tabLayPnlMain.Controls.Add(this.GrpBoxSelection, 1, 0);
            this.tabLayPnlMain.Controls.Add(this.GrpBoxMain, 0, 0);
            this.tabLayPnlMain.Controls.Add(this.ListViewPieceSelection, 1, 1);
            this.tabLayPnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLayPnlMain.Location = new System.Drawing.Point(0, 0);
            this.tabLayPnlMain.Name = "tabLayPnlMain";
            this.tabLayPnlMain.RowCount = 4;
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 350F));
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabLayPnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tabLayPnlMain.Size = new System.Drawing.Size(1184, 961);
            this.tabLayPnlMain.TabIndex = 1;
            // 
            // txtBoxLog
            // 
            this.tabLayPnlMain.SetColumnSpan(this.txtBoxLog, 2);
            this.txtBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBoxLog.Location = new System.Drawing.Point(3, 653);
            this.txtBoxLog.Multiline = true;
            this.txtBoxLog.Name = "txtBoxLog";
            this.txtBoxLog.ReadOnly = true;
            this.txtBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxLog.Size = new System.Drawing.Size(1178, 275);
            this.txtBoxLog.TabIndex = 1;
            // 
            // GrpBoxSelection
            // 
            this.GrpBoxSelection.Controls.Add(this.PicBoxSelection);
            this.GrpBoxSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GrpBoxSelection.Location = new System.Drawing.Point(837, 3);
            this.GrpBoxSelection.Name = "GrpBoxSelection";
            this.GrpBoxSelection.Size = new System.Drawing.Size(344, 344);
            this.GrpBoxSelection.TabIndex = 3;
            this.GrpBoxSelection.TabStop = false;
            this.GrpBoxSelection.Text = "Selection";
            // 
            // PicBoxSelection
            // 
            this.PicBoxSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicBoxSelection.Location = new System.Drawing.Point(3, 19);
            this.PicBoxSelection.Name = "PicBoxSelection";
            this.PicBoxSelection.Size = new System.Drawing.Size(338, 322);
            this.PicBoxSelection.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PicBoxSelection.TabIndex = 0;
            this.PicBoxSelection.TabStop = false;
            // 
            // GrpBoxMain
            // 
            this.GrpBoxMain.Controls.Add(this.PicBoxMaster);
            this.GrpBoxMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GrpBoxMain.Location = new System.Drawing.Point(3, 3);
            this.GrpBoxMain.Name = "GrpBoxMain";
            this.tabLayPnlMain.SetRowSpan(this.GrpBoxMain, 2);
            this.GrpBoxMain.Size = new System.Drawing.Size(828, 644);
            this.GrpBoxMain.TabIndex = 4;
            this.GrpBoxMain.TabStop = false;
            this.GrpBoxMain.Text = "Current Solution";
            // 
            // PicBoxMaster
            // 
            this.PicBoxMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicBoxMaster.Location = new System.Drawing.Point(3, 19);
            this.PicBoxMaster.Name = "PicBoxMaster";
            this.PicBoxMaster.Size = new System.Drawing.Size(822, 622);
            this.PicBoxMaster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PicBoxMaster.TabIndex = 2;
            this.PicBoxMaster.TabStop = false;
            this.PicBoxMaster.Click += new System.EventHandler(this.PicBoxMaster_Click);
            // 
            // ListViewPieceSelection
            // 
            this.ListViewPieceSelection.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.ListViewPieceSelection.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ListViewColOne});
            this.ListViewPieceSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListViewPieceSelection.FullRowSelect = true;
            this.ListViewPieceSelection.HideSelection = false;
            this.ListViewPieceSelection.Location = new System.Drawing.Point(837, 353);
            this.ListViewPieceSelection.MultiSelect = false;
            this.ListViewPieceSelection.Name = "ListViewPieceSelection";
            this.ListViewPieceSelection.ShowGroups = false;
            this.ListViewPieceSelection.Size = new System.Drawing.Size(344, 294);
            this.ListViewPieceSelection.TabIndex = 5;
            this.ListViewPieceSelection.UseCompatibleStateImageBehavior = false;
            this.ListViewPieceSelection.DoubleClick += new System.EventHandler(this.ListViewPieceSelection_DoubleClick);
            // 
            // ListViewColOne
            // 
            this.ListViewColOne.Name = "ListViewColOne";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 961);
            this.Controls.Add(this.tabLayPnlMain);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shredded & Scrambled";
            this.tabLayPnlMain.ResumeLayout(false);
            this.tabLayPnlMain.PerformLayout();
            this.GrpBoxSelection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PicBoxSelection)).EndInit();
            this.GrpBoxMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PicBoxMaster)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.TableLayoutPanel tabLayPnlMain;
        private System.Windows.Forms.TextBox txtBoxLog;
        private System.Windows.Forms.PictureBox PicBoxMaster;
        private System.Windows.Forms.GroupBox GrpBoxSelection;
        private System.Windows.Forms.PictureBox PicBoxSelection;
        private System.Windows.Forms.GroupBox GrpBoxMain;
        private System.Windows.Forms.ListView ListViewPieceSelection;
        private System.Windows.Forms.ColumnHeader ListViewColOne;
    }
}

