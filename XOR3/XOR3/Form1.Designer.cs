namespace XOR3 {
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
            this.txtBoxCypher = new System.Windows.Forms.TextBox();
            this.txtBoxPlain = new System.Windows.Forms.TextBox();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnReset = new System.Windows.Forms.Button();
            this.chkBoxBroken = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnChal159 = new System.Windows.Forms.Button();
            this.btnChal161 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtBoxCypher
            // 
            this.txtBoxCypher.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxCypher.Location = new System.Drawing.Point(12, 12);
            this.txtBoxCypher.Name = "txtBoxCypher";
            this.txtBoxCypher.Size = new System.Drawing.Size(760, 20);
            this.txtBoxCypher.TabIndex = 0;
            // 
            // txtBoxPlain
            // 
            this.txtBoxPlain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxPlain.Location = new System.Drawing.Point(12, 68);
            this.txtBoxPlain.Multiline = true;
            this.txtBoxPlain.Name = "txtBoxPlain";
            this.txtBoxPlain.ReadOnly = true;
            this.txtBoxPlain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxPlain.Size = new System.Drawing.Size(760, 223);
            this.txtBoxPlain.TabIndex = 1;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartStop.Location = new System.Drawing.Point(616, 326);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(75, 23);
            this.btnStartStop.TabIndex = 2;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 297);
            this.progressBar1.Maximum = 65536;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(760, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 3;
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Location = new System.Drawing.Point(697, 326);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 4;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // chkBoxBroken
            // 
            this.chkBoxBroken.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkBoxBroken.AutoSize = true;
            this.chkBoxBroken.Location = new System.Drawing.Point(12, 330);
            this.chkBoxBroken.Name = "chkBoxBroken";
            this.chkBoxBroken.Size = new System.Drawing.Size(120, 17);
            this.chkBoxBroken.TabIndex = 5;
            this.chkBoxBroken.Text = "Broken HEX Output";
            this.chkBoxBroken.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.btnChal159, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnChal161, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 35);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(760, 30);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // btnChal159
            // 
            this.btnChal159.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChal159.Location = new System.Drawing.Point(0, 0);
            this.btnChal159.Margin = new System.Windows.Forms.Padding(0);
            this.btnChal159.Name = "btnChal159";
            this.btnChal159.Size = new System.Drawing.Size(380, 30);
            this.btnChal159.TabIndex = 0;
            this.btnChal159.Text = "Set challenge 159 cypher text.";
            this.btnChal159.UseVisualStyleBackColor = true;
            this.btnChal159.Click += new System.EventHandler(this.btnChal159_Click);
            // 
            // btnChal161
            // 
            this.btnChal161.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChal161.Location = new System.Drawing.Point(380, 0);
            this.btnChal161.Margin = new System.Windows.Forms.Padding(0);
            this.btnChal161.Name = "btnChal161";
            this.btnChal161.Size = new System.Drawing.Size(380, 30);
            this.btnChal161.TabIndex = 1;
            this.btnChal161.Text = "Set challenge 161 cypher text.";
            this.btnChal161.UseVisualStyleBackColor = true;
            this.btnChal161.Click += new System.EventHandler(this.btnChal161_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 361);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.chkBoxBroken);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.txtBoxPlain);
            this.Controls.Add(this.txtBoxCypher);
            this.Name = "Form1";
            this.Text = "Challenge 159 & 161";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxCypher;
        private System.Windows.Forms.TextBox txtBoxPlain;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.CheckBox chkBoxBroken;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnChal159;
        private System.Windows.Forms.Button btnChal161;
    }
}

