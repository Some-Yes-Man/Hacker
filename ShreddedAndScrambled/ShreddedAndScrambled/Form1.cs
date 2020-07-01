using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace ShreddedAndScrambled {
    public partial class Form1 : Form {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        //private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\manypieces";
        private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\Pieces";
        //private const string PIECE_DIRECTORY = @"E:\Yes-Man\Downloads\Pieces";

        private BackgroundWorker imageAnalysisWorker;
        private BackgroundWorker crossAnalysisWorker;
        private BackgroundWorker puzzleSolverWorker;

        private readonly List<PuzzlePiece> pieceData = new List<PuzzlePiece>();
        private readonly Dictionary<byte, HashSet<PuzzlePiece>> hueHistogram = new Dictionary<byte, HashSet<PuzzlePiece>>();
        private readonly Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> counterEdgeMap = new Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>>();
        private readonly Dictionary<int, Dictionary<int, PuzzlePiece>> finishedPuzzle = new Dictionary<int, Dictionary<int, PuzzlePiece>>();

        private readonly Random prng = new Random();

        public Form1() {
            InitializeComponent();
        }

        private void Run_Click(object sender, EventArgs e) {
            this.pieceData.Clear();

            this.counterEdgeMap.Clear();
            this.counterEdgeMap.Add(Direction.NORTH, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.counterEdgeMap.Add(Direction.SOUTH, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.counterEdgeMap.Add(Direction.EAST, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.counterEdgeMap.Add(Direction.WEST, new Dictionary<byte, HashSet<PuzzlePiece>>());
            for (byte i = 0; i <= PuzzlePiece.EDGE_MATCH_BYTE; i++) {
                this.counterEdgeMap[Direction.NORTH].Add(i, new HashSet<PuzzlePiece>());
                this.counterEdgeMap[Direction.SOUTH].Add(i, new HashSet<PuzzlePiece>());
                this.counterEdgeMap[Direction.EAST].Add(i, new HashSet<PuzzlePiece>());
                this.counterEdgeMap[Direction.WEST].Add(i, new HashSet<PuzzlePiece>());
            }

            this.hueHistogram.Clear();
            for (int i = 0; i < 256; i++) {
                this.hueHistogram.Add((byte)i, new HashSet<PuzzlePiece>());
            }

            this.RunImageFileAnalysis();
        }

        private void RunImageFileAnalysis() {
            this.imageAnalysisWorker = new BackgroundWorker();
            this.imageAnalysisWorker.DoWork += this.ImageAnalysisWorker_DoWork;
            this.imageAnalysisWorker.ProgressChanged += this.ImageAnalysisWorker_ProgressChanged;
            this.imageAnalysisWorker.RunWorkerCompleted += this.ImageAnalysisWorker_RunWorkerCompleted;
            this.imageAnalysisWorker.WorkerReportsProgress = true;
            this.imageAnalysisWorker.RunWorkerAsync(this.pieceData);
        }

        private void ImageAnalysisWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            int counter = 0;
            int percentage = -1;
            string[] pieces = Directory.GetFiles(PIECE_DIRECTORY);
            int maxCounter = pieces.Length;

            // TMP .Take(50)
            foreach (string fileLocation in pieces) {
                PuzzlePiece piece = new PuzzlePiece(fileLocation);
                this.pieceData.Add(piece);

                // fill edge map
                if (!piece.PuzzleEdges.Contains(Direction.NORTH)) {
                    this.counterEdgeMap[Direction.NORTH][PuzzlePiece.InvertEdge(piece.NorthEdge)].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(Direction.SOUTH)) {
                    this.counterEdgeMap[Direction.SOUTH][PuzzlePiece.InvertEdge(piece.SouthEdge)].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(Direction.EAST)) {
                    this.counterEdgeMap[Direction.EAST][PuzzlePiece.InvertEdge(piece.EastEdge)].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(Direction.WEST)) {
                    this.counterEdgeMap[Direction.WEST][PuzzlePiece.InvertEdge(piece.WestEdge)].Add(piece);
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

        private void ImageAnalysisWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            LOGGER.Info("Completed {percentage}% of file read job.", e.ProgressPercentage);
        }

        private void ImageAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            LOGGER.Info("Initial analysis done!");

            foreach (KeyValuePair<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> direction in this.counterEdgeMap) {
                int dirMin = int.MaxValue;
                int dirMax = int.MinValue;
                foreach (KeyValuePair<byte, HashSet<PuzzlePiece>> edge in direction.Value) {
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

            if (!this.pieceData.Any(x => x.PuzzleEdges.Count != 0)) {
                LOGGER.Info("Found NO edges or corners!");
            }

            LOGGER.Info("Running cross-image analysis.");
            this.RunCrossImageAnalysis();
        }

        private void RunCrossImageAnalysis() {
            this.crossAnalysisWorker = new BackgroundWorker();
            this.crossAnalysisWorker.DoWork += this.CrossAnalysisWorker_DoWork;
            this.crossAnalysisWorker.ProgressChanged += this.CrossAnalysisWorker_ProgressChanged;
            this.crossAnalysisWorker.RunWorkerCompleted += this.CrossAnalysisWorker_RunWorkerCompleted;
            this.crossAnalysisWorker.WorkerReportsProgress = true;
            this.crossAnalysisWorker.RunWorkerAsync(this.pieceData);
        }

        private void CrossAnalysisWorker_DoWork(object sender, DoWorkEventArgs e) {
            //BackgroundWorker worker = sender as BackgroundWorker;
        }

        private void CrossAnalysisWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            LOGGER.Info("Completed {percentage}% of cross-image analysis.");
        }

        private void CrossAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            LOGGER.Info("Cross-image analysis done.");

            LOGGER.Info("Starting puzzle solver.");
            this.RunPuzzleSolver();
        }

        private void RunPuzzleSolver() {
            this.puzzleSolverWorker = new BackgroundWorker();
            this.puzzleSolverWorker.DoWork += this.PuzzleSolverWorker_DoWork;
            this.puzzleSolverWorker.ProgressChanged += this.PuzzleSolverWorker_ProgressChanged;
            this.puzzleSolverWorker.RunWorkerCompleted += this.PuzzleSolverWorker_RunWorkerCompleted;
            this.puzzleSolverWorker.WorkerReportsProgress = true;
            this.puzzleSolverWorker.RunWorkerAsync();
        }

        private void PuzzleSolverWorker_DoWork(object sender, DoWorkEventArgs e) {
            // pick a random start piece
            PuzzlePiece randomStartPiece = this.pieceData[this.prng.Next(this.pieceData.Count)];
            //this.AddPieceToFinishedPuzzle(0, 0, randomStartPiece);
            this.AddPieceToFinishedPuzzle(0, 0, this.pieceData.Find(x => x.PuzzleEdges.Contains(Direction.NORTH) && x.PuzzleEdges.Contains(Direction.WEST)));

            List<MissingPiece> missingNeighboringPieces = new List<MissingPiece>();
            // iterate all piece that are done
            foreach (KeyValuePair<int, Dictionary<int, PuzzlePiece>> finishedColumn in this.finishedPuzzle) {
                foreach (KeyValuePair<int, PuzzlePiece> finishedPiece in finishedColumn.Value) {
                    // add every missing neighbor of the current piece
                    foreach (Tuple<int, int> emptySpot in this.GetMissingNeighbors(finishedColumn.Key, finishedPiece.Key)) {
                        MissingPiece missingPiece = new MissingPiece {
                            X = emptySpot.Item1,
                            Y = emptySpot.Item2,
                            Edges = this.GetSurroundingEdges(emptySpot.Item1, emptySpot.Item2)
                        };
                        // all physically matching pieces
                        IEnumerable<PuzzlePiece> physicalMatches = this.pieceData.Where(x =>
                            (!missingPiece.Edges.ContainsKey(Direction.NORTH) || this.counterEdgeMap[Direction.NORTH][missingPiece.Edges[Direction.NORTH]].Contains(x)) &&
                            (!missingPiece.Edges.ContainsKey(Direction.SOUTH) || this.counterEdgeMap[Direction.SOUTH][missingPiece.Edges[Direction.SOUTH]].Contains(x)) &&
                            (!missingPiece.Edges.ContainsKey(Direction.EAST) || this.counterEdgeMap[Direction.EAST][missingPiece.Edges[Direction.EAST]].Contains(x)) &&
                            (!missingPiece.Edges.ContainsKey(Direction.WEST) || this.counterEdgeMap[Direction.WEST][missingPiece.Edges[Direction.WEST]].Contains(x))
                        );
                        // plus their optical match
                        List<Tuple<PuzzlePiece, int>> sortedMatches = new List<Tuple<PuzzlePiece, int>>();
                        foreach (PuzzlePiece physicallyMatchingPiece in physicalMatches) {
                            int opticalDistance = 0;
                            if (missingPiece.Edges.ContainsKey(Direction.NORTH)) {
                                PuzzlePiece northernPiece = this.finishedPuzzle[emptySpot.Item1][emptySpot.Item2 - 1];
                                opticalDistance += PuzzlePiece.GetHslDistance(physicallyMatchingPiece.NorthKeys, northernPiece.SouthKeys);
                                opticalDistance += PuzzlePiece.GetRgbDistance(physicallyMatchingPiece.NorthKeys, northernPiece.SouthKeys);
                            }
                            if (missingPiece.Edges.ContainsKey(Direction.SOUTH)) {
                                PuzzlePiece southernPiece = this.finishedPuzzle[emptySpot.Item1][emptySpot.Item2 + 1];
                                opticalDistance += PuzzlePiece.GetHslDistance(physicallyMatchingPiece.SouthKeys, southernPiece.NorthKeys);
                                opticalDistance += PuzzlePiece.GetRgbDistance(physicallyMatchingPiece.SouthKeys, southernPiece.NorthKeys);
                            }
                            if (missingPiece.Edges.ContainsKey(Direction.EAST)) {
                                PuzzlePiece easternPiece = this.finishedPuzzle[emptySpot.Item1 + 1][emptySpot.Item2];
                                opticalDistance += PuzzlePiece.GetHslDistance(physicallyMatchingPiece.EastKeys, easternPiece.WestKeys);
                                opticalDistance += PuzzlePiece.GetRgbDistance(physicallyMatchingPiece.EastKeys, easternPiece.WestKeys);
                            }
                            if (missingPiece.Edges.ContainsKey(Direction.WEST)) {
                                PuzzlePiece westernPiece = this.finishedPuzzle[emptySpot.Item1 - 1][emptySpot.Item2];
                                opticalDistance += PuzzlePiece.GetHslDistance(physicallyMatchingPiece.WestKeys, westernPiece.EastKeys);
                                opticalDistance += PuzzlePiece.GetRgbDistance(physicallyMatchingPiece.WestKeys, westernPiece.EastKeys);
                            }
                            sortedMatches.Add(new Tuple<PuzzlePiece, int>(physicallyMatchingPiece, opticalDistance));
                        }
                        // sort by optical difference
                        missingPiece.MatchingPieces.AddRange(sortedMatches.OrderBy(x => x.Item2).ToList());
                        missingNeighboringPieces.Add(missingPiece);
                    }
                }
            }

            LOGGER.Info("foo");
        }

        private void PuzzleSolverWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            LOGGER.Info("Solving ... {percentage}%", e.ProgressPercentage);
        }

        private void PuzzleSolverWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            LOGGER.Info("DONE!!");
        }

        private void AddPieceToFinishedPuzzle(int x, int y, PuzzlePiece piece) {
            if (!this.finishedPuzzle.ContainsKey(x)) {
                this.finishedPuzzle.Add(x, new Dictionary<int, PuzzlePiece>());
            }
            if (!this.finishedPuzzle[x].ContainsKey(y)) {
                this.finishedPuzzle[x].Add(y, piece);
            }
            else {
                throw new InvalidOperationException("There is already a puzzle piece at position " + x + ":" + y);
            }
        }

        private HashSet<Tuple<int, int>> GetMissingNeighbors(int x, int y) {
            if (!this.finishedPuzzle.ContainsKey(x) || !this.finishedPuzzle[x].ContainsKey(y)) {
                throw new InvalidOperationException("There is no piece at the given position.");
            }
            PuzzlePiece piece = this.finishedPuzzle[x][y];
            HashSet<Tuple<int, int>> neighbors = new HashSet<Tuple<int, int>>();
            // north
            if (!piece.PuzzleEdges.Contains(Direction.NORTH) && (!this.finishedPuzzle.ContainsKey(x) || !this.finishedPuzzle[x].ContainsKey(y - 1))) {
                neighbors.Add(new Tuple<int, int>(x, y - 1));
            }
            // south
            if (!piece.PuzzleEdges.Contains(Direction.SOUTH) && (!this.finishedPuzzle.ContainsKey(x) || !this.finishedPuzzle[x].ContainsKey(y + 1))) {
                neighbors.Add(new Tuple<int, int>(x, y + 1));
            }
            // east
            if (!piece.PuzzleEdges.Contains(Direction.EAST) && (!this.finishedPuzzle.ContainsKey(x + 1) || !this.finishedPuzzle[x + 1].ContainsKey(y))) {
                neighbors.Add(new Tuple<int, int>(x + 1, y));
            }
            // west
            if (!piece.PuzzleEdges.Contains(Direction.WEST) && (!this.finishedPuzzle.ContainsKey(x - 1) || !this.finishedPuzzle[x - 1].ContainsKey(y))) {
                neighbors.Add(new Tuple<int, int>(x - 1, y));
            }
            return neighbors;
        }

        private Dictionary<Direction, byte> GetSurroundingEdges(int x, int y) {
            Dictionary<Direction, byte> surroundingEdges = new Dictionary<Direction, byte>();

            if (this.finishedPuzzle.ContainsKey(x) && this.finishedPuzzle[x].ContainsKey(y - 1)) {
                surroundingEdges.Add(Direction.NORTH, this.finishedPuzzle[x][y - 1].SouthEdge);
            }
            if (this.finishedPuzzle.ContainsKey(x) && this.finishedPuzzle[x].ContainsKey(y + 1)) {
                surroundingEdges.Add(Direction.SOUTH, this.finishedPuzzle[x][y + 1].NorthEdge);
            }
            if (this.finishedPuzzle.ContainsKey(x + 1) && this.finishedPuzzle[x + 1].ContainsKey(y)) {
                surroundingEdges.Add(Direction.EAST, this.finishedPuzzle[x + 1][y].WestEdge);
            }
            if (this.finishedPuzzle.ContainsKey(x - 1) && this.finishedPuzzle[x - 1].ContainsKey(y)) {
                surroundingEdges.Add(Direction.WEST, this.finishedPuzzle[x - 1][y].EastEdge);
            }

            return surroundingEdges;
        }

    }
}
