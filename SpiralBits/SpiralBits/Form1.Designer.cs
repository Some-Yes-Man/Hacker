namespace SpiralBits {
    partial class FormSpiralBits {
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
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.picBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).BeginInit();
            this.SuspendLayout();
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "png";
            this.saveFileDialog1.Filter = "PNG Files|*.png";
            // 
            // picBox
            // 
            this.picBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picBox.Image = global::SpiralBits.Properties.Resources.spiral4;
            this.picBox.Location = new System.Drawing.Point(0, 0);
            this.picBox.Name = "picBox";
            this.picBox.Size = new System.Drawing.Size(1084, 1061);
            this.picBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picBox.TabIndex = 0;
            this.picBox.TabStop = false;
            this.picBox.Click += new System.EventHandler(this.picBox_Click);
            // 
            // FormSpiralBits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 1061);
            this.Controls.Add(this.picBox);
            this.Name = "FormSpiralBits";
            this.Text = "SpiralBits";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picBox;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}

