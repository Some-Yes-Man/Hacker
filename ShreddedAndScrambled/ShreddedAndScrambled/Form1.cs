using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ShreddedAndScrambled {
    public partial class Form1 : Form {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        //private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\manypieces";
        private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\Pieces";

        private BackgroundWorker imageAnalysisWorker;
        private BackgroundWorker puzzleSolverWorker;

        private readonly List<PieceData> pieceData = new List<PieceData>();
        private readonly Dictionary<Tuple<int, int>, PieceData> finishedPuzzle = new Dictionary<Tuple<int, int>, PieceData>();

        public Form1() {
            InitializeComponent();
        }

        private void Run_Click(object sender, EventArgs e) {
            this.pieceData.Clear();
            this.RunImageFileAnalysis();
        }

        private void RunImageFileAnalysis() {
            this.imageAnalysisWorker = new BackgroundWorker();
            this.imageAnalysisWorker.DoWork += this.imageAnalysisWorker_DoWork;
            this.imageAnalysisWorker.RunWorkerCompleted += this.imageAnalysisWorker_RunWorkerCompleted;
            this.imageAnalysisWorker.ProgressChanged += this.imageAnalysisWorker_ProgressChanged;
            this.imageAnalysisWorker.WorkerReportsProgress = true;
            this.imageAnalysisWorker.RunWorkerAsync(this.pieceData);
        }

        private void imageAnalysisWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            List<PieceData> pieceList = e.Argument as List<PieceData>;

            int counter = 0;
            int percentage = -1;
            string[] pieces = Directory.GetFiles(PIECE_DIRECTORY);
            int maxCounter = pieces.Length;

            // TMP .Take(50)
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

        private void imageAnalysisWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            LOGGER.Info("Completed {percentage}% of file read job.", e.ProgressPercentage);
        }

        private void imageAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            LOGGER.Info("Done!");

            if (!this.pieceData.Any(x => x.PuzzleEdges.Count != 0)) {
                LOGGER.Info("Found NO edges or corners!");
            }

            //foreach (PieceData piece in this.pieceData) {
            //    LOGGER.Info("{file} aR:{aR} aG:{aG} aB:{aB} dR:{dR} dG:{dG} dB:{dB}", piece.Filename, piece.AverageColor.R, piece.AverageColor.G, piece.AverageColor.B, piece.DeviationColor.R, piece.DeviationColor.G, piece.DeviationColor.B);
            //}

            LOGGER.Info("Starting puzzle solver.");
            this.RunClassicalPuzzleSolver();
        }

        private void RunClassicalPuzzleSolver() {
            // place upper left corner
            PieceData upperLeftCorner = this.pieceData.First(x => x.PuzzleEdges.Contains(PieceData.Direction.NORTH) && x.PuzzleEdges.Contains(PieceData.Direction.WEST));
            upperLeftCorner.AlreadyUsed = true;
            this.finishedPuzzle.Add(new Tuple<int, int>(0, 0), upperLeftCorner);

            foreach (KeyValuePair<Tuple<int,int>,PieceData> finished in this.finishedPuzzle) {
                PieceData finishedPiece = finished.Value;
                //...
            }
        }

    }
}
