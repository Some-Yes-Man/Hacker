namespace Password3 {
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
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.txtBoxChallenge = new TextBox();
            this.txtBoxPassword = new TextBox();
            this.button1 = new Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 23F));
            this.tableLayoutPanel1.Controls.Add(this.txtBoxChallenge, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtBoxPassword, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.button1, 0, 2);
            this.tableLayoutPanel1.Dock = DockStyle.Fill;
            this.tableLayoutPanel1.Location = new Point(0, 0);
            this.tableLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            this.tableLayoutPanel1.Size = new Size(284, 161);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // txtBoxChallenge
            // 
            this.txtBoxChallenge.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.txtBoxChallenge.Location = new Point(3, 12);
            this.txtBoxChallenge.Margin = new Padding(3, 4, 3, 4);
            this.txtBoxChallenge.Name = "txtBoxChallenge";
            this.txtBoxChallenge.Size = new Size(278, 28);
            this.txtBoxChallenge.TabIndex = 0;
            this.txtBoxChallenge.TextAlign = HorizontalAlignment.Center;
            this.txtBoxChallenge.TextChanged += this.txtBoxChallenge_TextChanged;
            // 
            // txtBoxPassword
            // 
            this.txtBoxPassword.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.txtBoxPassword.Location = new Point(3, 65);
            this.txtBoxPassword.Margin = new Padding(3, 4, 3, 4);
            this.txtBoxPassword.Name = "txtBoxPassword";
            this.txtBoxPassword.ReadOnly = true;
            this.txtBoxPassword.Size = new Size(278, 28);
            this.txtBoxPassword.TabIndex = 1;
            this.txtBoxPassword.TextAlign = HorizontalAlignment.Center;
            // 
            // button1
            // 
            this.button1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.button1.Location = new Point(3, 118);
            this.button1.Margin = new Padding(3, 4, 3, 4);
            this.button1.Name = "button1";
            this.button1.Size = new Size(278, 31);
            this.button1.TabIndex = 2;
            this.button1.Text = "Download && Decompile";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(284, 161);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new Font("Source Sans Pro", 12F, FontStyle.Regular, GraphicsUnit.Point);
            this.Margin = new Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Text = "Password Decompiler";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TextBox txtBoxChallenge;
        private TextBox txtBoxPassword;
        private Button button1;
    }
}