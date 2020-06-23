using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace XOR3 {
    public partial class Form1 : Form {
        private int startB = 0;
        private int runningB = 0;
        private int keyX;
        private byte[] cypherBytes;
        private int cypherLength = 0;
        private byte[] plainBytes;
        private int plainLength = 0;
        // \"\\.\\+\\*\\?\\^\\$\\[\\]\\{\\}\\(\\)\\|\\/
        private static string validPattern = "^[a-zA-Z0-9,! '#%&-:;<>=@_Æ]+$";
        private BackgroundWorker worker = new BackgroundWorker();

        public Form1() {
            InitializeComponent();
            this.worker.WorkerSupportsCancellation = true;
            this.worker.WorkerReportsProgress = true;
            this.worker.ProgressChanged += worker_ProgressChanged;
            this.worker.DoWork += worker_DoWork;
            this.worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e) {
            // try all the start Bs
            while ((this.startB < 256) && !e.Cancel) {
                // try all the keys
                this.keyX = 0;
                while ((this.keyX < 256) && !e.Cancel) {
                    // reset B
                    this.runningB = this.startB;
                    bool validResult = true;
                    byte tmpPlainByte;
                    // all the chars in the cypher text
                    int cypherTextIndex = 0;
                    while ((cypherTextIndex < txtBoxCypher.Text.Length) && validResult) {
                        // read one char from cypher
                        this.cypherBytes[cypherLength] = (cypherTextIndex + 1 < txtBoxCypher.Text.Length) ? Convert.ToByte(txtBoxCypher.Text.Substring(cypherTextIndex, 2), 16) : Convert.ToByte("0" + txtBoxCypher.Text.Substring(cypherTextIndex, 1), 16);
                        // actual decryption
                        tmpPlainByte = (byte)(this.cypherBytes[cypherLength] ^ this.runningB);
                        // check if the result is valid
                        if (Regex.IsMatch(Encoding.UTF8.GetString(new byte[1] { tmpPlainByte }), validPattern)) {
                            this.plainBytes[this.plainLength] = tmpPlainByte;
                            // update B
                            this.runningB = (this.runningB + this.keyX) % 256;
                            // skip to next byte
                            this.cypherLength++;
                            this.plainLength++;
                            cypherTextIndex += 2;
                        }
                        else {
                            if ((this.startB == 249) && (this.keyX == 67)) {
                                Console.WriteLine("Probably a missing char in RegEx: '" + Encoding.UTF8.GetString(new byte[1] { tmpPlainByte }) + "' (#" + tmpPlainByte + ").");
                            }
                            // check for broken (if selected)
                            if (chkBoxBroken.Checked) {
                                // read one char from cypher
                                this.cypherBytes[cypherLength] = Convert.ToByte("0" + txtBoxCypher.Text.Substring(cypherTextIndex, 1), 16);
                                // actual decryption
                                tmpPlainByte = (byte)(this.cypherBytes[cypherLength] ^ this.runningB);
                                // check if the result is valid
                                if (Regex.IsMatch(Encoding.UTF8.GetString(new byte[1] { tmpPlainByte }), validPattern)) {
                                    this.plainBytes[this.plainLength] = tmpPlainByte;
                                    // update B
                                    this.runningB = (this.runningB + this.keyX) % 256;
                                    // skip to next byte
                                    this.cypherLength++;
                                    this.plainLength++;
                                    cypherTextIndex += 1;
                                }
                                else {
                                    if ((this.startB == 249) && (this.keyX == 67)) {
                                        Console.WriteLine("Probably a missing char in RegEx: '" + Encoding.UTF8.GetString(new byte[1] { tmpPlainByte }) + "' (#" + tmpPlainByte + ").");
                                    }
                                    validResult = false;
                                }
                            }
                            else {
                                validResult = false;
                            }
                        }
                    }
                    string plainText = string.Empty;
                    //if (validResult) {
                    if (validResult || ((this.startB == 249) && (this.keyX == 67))) {
                        plainText = "B: " + this.startB + " - K: " + this.keyX + " - Result: " + Encoding.UTF8.GetString(this.plainBytes.Take(plainLength).ToArray());
                    }
                    ((BackgroundWorker)sender).ReportProgress(this.startB * 256 + this.keyX, plainText);
                    this.cypherLength = 0;
                    this.plainLength = 0;
                    this.keyX++;
                }
                this.startB++;
                // temp
                //return;
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            this.progressBar1.PerformStep();
            // evaluate result
            if ((string)e.UserState != string.Empty) {
                txtBoxPlain.AppendText((string)e.UserState + Environment.NewLine);
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            btnStartStop.Text = "Start";
            progressBar1.Value = 0;
            btnReset.Enabled = true;
            txtBoxCypher.Enabled = true;
        }

        private void btnStartStop_Click(object sender, EventArgs e) {
            if (this.worker.IsBusy) {
                btnStartStop.Text = "Start";
                btnReset.Enabled = true;
                txtBoxCypher.Enabled = true;
                this.worker.CancelAsync();
            }
            else {
                if (!chkBoxBroken.Checked && (txtBoxCypher.Text.Length % 2 == 1)) {
                    txtBoxPlain.Clear();
                    txtBoxPlain.AppendText("Odd cypher text length. Aborting ...");
                }
                else {
                    // GUI
                    btnStartStop.Text = "Stop";
                    btnReset.Enabled = false;
                    txtBoxCypher.Enabled = false;
                    // init fields
                    this.cypherBytes = new byte[txtBoxCypher.Text.Length * 2];
                    this.plainBytes = new byte[txtBoxCypher.Text.Length * 2];
                    // run
                    this.worker.RunWorkerAsync();
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e) {
            this.startB = 0;
            this.runningB = 0;
            this.keyX = 0;
            this.cypherLength = 0;
            this.plainLength = 0;
        }

        private void btnChal159_Click(object sender, EventArgs e) {
            txtBoxCypher.Text = "31cf55aa0c91fb6fcb33f34793fe00c72ebc4c88fd57dc6ba71e71b759d83588";
            chkBoxBroken.Checked = false;
        }

        private void btnChal161_Click(object sender, EventArgs e) {
            txtBoxCypher.Text = "8d541ae26426f8b97426b7ae7240d78e401f8f904717d09b2fa4a4622cfcbf7337fbba2cdbcb4e3cdb994812b66a27e9e02f21faf8712bd2907fc384564998857e3b1";
            chkBoxBroken.Checked = true;
        }
    }
}
