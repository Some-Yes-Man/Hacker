namespace StemSearch {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.txtBoxBefore = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblLinesAfter = new System.Windows.Forms.Label();
            this.txtBoxAfter = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.lblLinesBefore = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtBoxBefore
            // 
            this.txtBoxBefore.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.txtBoxBefore, 3);
            this.txtBoxBefore.Location = new System.Drawing.Point(3, 3);
            this.txtBoxBefore.MaxLength = 500000;
            this.txtBoxBefore.Multiline = true;
            this.txtBoxBefore.Name = "txtBoxBefore";
            this.txtBoxBefore.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxBefore.Size = new System.Drawing.Size(879, 264);
            this.txtBoxBefore.TabIndex = 0;
            this.txtBoxBefore.TextChanged += new System.EventHandler(this.txtBoxBefore_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.lblLinesAfter, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtBoxBefore, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtBoxAfter, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnRun, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblLinesBefore, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(885, 570);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // lblLinesAfter
            // 
            this.lblLinesAfter.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblLinesAfter.AutoSize = true;
            this.lblLinesAfter.Location = new System.Drawing.Point(681, 278);
            this.lblLinesAfter.Name = "lblLinesAfter";
            this.lblLinesAfter.Size = new System.Drawing.Size(113, 13);
            this.lblLinesAfter.TabIndex = 5;
            this.lblLinesAfter.Text = "Lines of code (after): 0";
            // 
            // txtBoxAfter
            // 
            this.txtBoxAfter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.txtBoxAfter, 3);
            this.txtBoxAfter.Location = new System.Drawing.Point(3, 303);
            this.txtBoxAfter.MaxLength = 500000;
            this.txtBoxAfter.Multiline = true;
            this.txtBoxAfter.Name = "txtBoxAfter";
            this.txtBoxAfter.ReadOnly = true;
            this.txtBoxAfter.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxAfter.Size = new System.Drawing.Size(879, 264);
            this.txtBoxAfter.TabIndex = 1;
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Location = new System.Drawing.Point(298, 273);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(289, 24);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "Optimize!";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // lblLinesBefore
            // 
            this.lblLinesBefore.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblLinesBefore.AutoSize = true;
            this.lblLinesBefore.Location = new System.Drawing.Point(91, 278);
            this.lblLinesBefore.Name = "lblLinesBefore";
            this.lblLinesBefore.Size = new System.Drawing.Size(113, 13);
            this.lblLinesBefore.TabIndex = 4;
            this.lblLinesBefore.Text = "Lines of code (after): 0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(909, 594);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxBefore;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox txtBoxAfter;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label lblLinesBefore;
        private System.Windows.Forms.Label lblLinesAfter;
    }
}

