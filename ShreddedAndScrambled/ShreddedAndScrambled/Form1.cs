using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ShreddedAndScrambled {
    public partial class Form1 : Form {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\manypieces";

        private BackgroundWorker backgroundWorker;

        private List<PieceData> pieceData;

        public Form1() {
            InitializeComponent();
        }

        private void Run_Click(object sender, EventArgs e) {
            this.pieceData = new List<PieceData>();

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += this.BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += this.BackgroundWorker_RunWorkerCompleted;
            backgroundWorker.ProgressChanged += this.BackgroundWorker_ProgressChanged;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerAsync(this.pieceData);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            List<PieceData> pieceList = e.Argument as List<PieceData>;

            int counter = 0;
            int percentage = -1;
            string[] pieces = Directory.GetFiles(PIECE_DIRECTORY);
            int maxCounter = pieces.Length;

            foreach (string fileLocation in pieces) {
                pieceList.Add(new PieceData(fileLocation));

                counter++;
                if (Math.Floor((double)counter * 100 / maxCounter) > percentage) {
                    if (percentage > -1) {
                        worker.ReportProgress(percentage);
                    }
                    percentage++;
                }
            }
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            LOGGER.Info("Completed {percentage}% of file read job.", e.ProgressPercentage);
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            LOGGER.Info("Done!");
        }

    }
}
