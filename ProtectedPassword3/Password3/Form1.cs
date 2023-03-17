using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Password3 {
    public partial class Form1 : Form {

        // https://github.com/intoolswetrust/jd-cli
        const string jdCliLocation = "C:\\Users\\robert.krausse\\Downloads\\jd-cli-build\\jd-cli.bat";

        public Form1() {
            InitializeComponent();
        }

        private async void txtBoxChallenge_TextChanged(object sender, EventArgs e) {
            // www.hacker.org/challenge/misc/pp3/x.php?x=1039572075
            using (HttpClient client = new HttpClient()) {
                UriBuilder requestBuilder = new UriBuilder("https://www.hacker.org/challenge/misc/pp3/x.php");
                requestBuilder.Query = "x=" + txtBoxChallenge.Text;
                HttpResponseMessage response = await client.GetAsync(requestBuilder.Uri);
                using (Stream responseStream = response.Content.ReadAsStream()) {
                    using (FileStream fileStream = File.Create("password3.jar")) {
                        responseStream.CopyTo(fileStream);
                    }
                }
            }

            // ./hacker/chal/prot/PasswordProtector$1.class (+PasswordProtector$2.class +PasswordProtector.class)
            ZipFile.ExtractToDirectory("password3.jar", ".", true);

            string batchProcessOutput = string.Empty;

            Process batchProcess = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/C " + jdCliLocation + " ./hacker/chal/prot/PasswordProtector$1.class");
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            batchProcess.StartInfo = processStartInfo;
            batchProcess.OutputDataReceived += (sender, args) => batchProcessOutput += args.Data;
            batchProcess.Start();
            batchProcess.BeginOutputReadLine();
            batchProcess.WaitForExit();

            Match passwordMatch = Regex.Match(batchProcessOutput, "Integer\\.toString\\([-]?(\\d+)\\);");
            if ((passwordMatch != null) && passwordMatch.Success) {
                txtBoxPassword.Text = passwordMatch.Groups[1].Value;
            }
            else {
                txtBoxPassword.Text = "Failed.";
            }
        }
    }
}
