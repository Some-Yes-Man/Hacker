using NLog;
using NLog.Fluent;
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
        //private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\Pieces";
        private const string PIECE_DIRECTORY = @"E:\Yes-Man\Downloads\Pieces";

        private BackgroundWorker imageAnalysisWorker;
        private BackgroundWorker crossAnalysisWorker;
        private BackgroundWorker puzzleSolverWorker;

        private readonly List<PieceData> pieceData = new List<PieceData>();
        private readonly Dictionary<byte, HashSet<PieceData>> hueHistogram = new Dictionary<byte, HashSet<PieceData>>();
        private readonly Dictionary<PieceData.Direction, Dictionary<byte, HashSet<PieceData>>> edgeMap = new Dictionary<PieceData.Direction, Dictionary<byte, HashSet<PieceData>>>();
        private readonly Dictionary<Tuple<int, int>, PieceData> finishedPuzzle = new Dictionary<Tuple<int, int>, PieceData>();

        public Form1() {
            InitializeComponent();
        }

        private void Run_Click(object sender, EventArgs e) {
            this.pieceData.Clear();

            this.edgeMap.Add(PieceData.Direction.NORTH, new Dictionary<byte, HashSet<PieceData>>());
            this.edgeMap.Add(PieceData.Direction.SOUTH, new Dictionary<byte, HashSet<PieceData>>());
            this.edgeMap.Add(PieceData.Direction.EAST, new Dictionary<byte, HashSet<PieceData>>());
            this.edgeMap.Add(PieceData.Direction.WEST, new Dictionary<byte, HashSet<PieceData>>());
            for (byte i = 0; i <= PieceData.EDGE_MATCH_BYTE; i++) {
                this.edgeMap[PieceData.Direction.NORTH].Add(i, new HashSet<PieceData>());
                this.edgeMap[PieceData.Direction.SOUTH].Add(i, new HashSet<PieceData>());
                this.edgeMap[PieceData.Direction.EAST].Add(i, new HashSet<PieceData>());
                this.edgeMap[PieceData.Direction.WEST].Add(i, new HashSet<PieceData>());
            }

            for (int i = 0; i < 256; i++) {
                this.hueHistogram.Add((byte)i, new HashSet<PieceData>());
            }

            this.RunImageFileAnalysis();
            this.RunCrossImageAnalysis();
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

            int counter = 0;
            int percentage = -1;
            string[] pieces = Directory.GetFiles(PIECE_DIRECTORY);
            int maxCounter = pieces.Length;

            // TMP .Take(50)
            foreach (string fileLocation in pieces) {
                PieceData piece = new PieceData(fileLocation);
                this.pieceData.Add(piece);

                // fill edge map
                if (!piece.PuzzleEdges.Contains(PieceData.Direction.NORTH)) {
                    this.edgeMap[PieceData.Direction.NORTH][piece.NorthEdge].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(PieceData.Direction.SOUTH)) {
                    this.edgeMap[PieceData.Direction.SOUTH][piece.SouthEdge].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(PieceData.Direction.EAST)) {
                    this.edgeMap[PieceData.Direction.EAST][piece.EastEdge].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(PieceData.Direction.WEST)) {
                    this.edgeMap[PieceData.Direction.WEST][piece.WestEdge].Add(piece);
                }

                // fill diagrams
                byte normalizedAverageHue = (byte)Math.Floor(piece.AverageColor.GetHue() / 360 * 255);
                this.hueHistogram[normalizedAverageHue].Add(piece);

                counter++;
                if (Math.Floor((double)counter * 100 / maxCounter) > percentage) {
                    if (percentage > 0 && percentage % 10 == 0) {
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
            LOGGER.Info("Initial analysis done!");

            foreach (KeyValuePair<PieceData.Direction, Dictionary<byte, HashSet<PieceData>>> direction in this.edgeMap) {
                int dirMin = int.MaxValue;
                int dirMax = int.MinValue;
                foreach (KeyValuePair<byte, HashSet<PieceData>> edge in direction.Value) {
                    int edgeCount = edge.Value.Count;
                    if (edgeCount < dirMin) {
                        dirMin = edgeCount;
                    }
                    if (edgeCount > dirMax) {
                        dirMax = edgeCount;
                    }
                    LOGGER.Trace("{direction} {byte} {pieceCount}", direction.Key, edge.Key, edgeCount);
                }
                LOGGER.Info("{direction} {min}-{max}", direction.Key, dirMin, dirMax);
            }

            for (int h = 0; h <= 255; h++) {
                LOGGER.Info("{hue} : {count}", h, this.hueHistogram[(byte)h].Count);
            }

            if (!this.pieceData.Any(x => x.PuzzleEdges.Count != 0)) {
                LOGGER.Info("Found NO edges or corners!");
            }

            //foreach (PieceData piece in this.pieceData) {
            //    LOGGER.Info("{file} aR:{aR} aG:{aG} aB:{aB} dR:{dR} dG:{dG} dB:{dB}", piece.Filename, piece.AverageColor.R, piece.AverageColor.G, piece.AverageColor.B, piece.DeviationColor.R, piece.DeviationColor.G, piece.DeviationColor.B);
            //}

            LOGGER.Info("Running cross-image analysis.");
            this.RunCrossImageAnalysis();
        }

        private void RunCrossImageAnalysis() {
            //this.crossAnalysisWorker = new BackgroundWorker();
            //this.crossAnalysisWorker.DoWork += this.CrossAnalysisWorker_DoWork;
            //this.crossAnalysisWorker.RunWorkerCompleted += this.CrossAnalysisWorker_RunWorkerCompleted;
            //this.crossAnalysisWorker.ProgressChanged += this.CrossAnalysisWorker_ProgressChanged;
            //this.crossAnalysisWorker.WorkerReportsProgress = true;
            //this.crossAnalysisWorker.RunWorkerAsync(this.pieceData);
        }

        private void CrossAnalysisWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;


        }

        private void CrossAnalysisWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            LOGGER.Info("Completed {percentage}% of cross-image analysis.");
        }

        private void CrossAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            LOGGER.Info("Cross-image analysis done.");

            LOGGER.Info("Starting puzzle solver.");
            this.RunClassicalPuzzleSolver();
        }

        private void RunClassicalPuzzleSolver() {
            // place upper left corner
            PieceData upperLeftCorner = this.pieceData.First(x => x.PuzzleEdges.Contains(PieceData.Direction.NORTH) && x.PuzzleEdges.Contains(PieceData.Direction.WEST));
            upperLeftCorner.AlreadyUsed = true;
            this.finishedPuzzle.Add(new Tuple<int, int>(0, 0), upperLeftCorner);

            foreach (KeyValuePair<Tuple<int, int>, PieceData> finished in this.finishedPuzzle) {
                PieceData finishedPiece = finished.Value;
                //...
            }
        }

    }
}
