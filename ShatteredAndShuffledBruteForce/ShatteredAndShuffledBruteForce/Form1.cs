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

namespace ShatteredAndShuffledBruteForce {
    public partial class Form1 : Form {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private const string PIECE_DIRECTORY = @"E:\Yes-Man\Downloads\Pieces";

        private BackgroundWorker imageAnalysisWorker;
        private BackgroundWorker borderSolverWorker;
        private BackgroundWorker puzzleSolverWorker;

        private readonly List<PuzzlePiece> pieceData = new List<PuzzlePiece>();
        private readonly Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> mainCounterEdgeMap = new Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>>();
        private readonly Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> borderCounterEdgeMap = new Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>>();

        private void LogWithBox(LogLevel level, string message, params object[] args) {
            if ((level == LogLevel.Info) || (level == LogLevel.Warn) || (level == LogLevel.Error)) {
                txtBoxLog.AppendText(string.Format(message, args) + Environment.NewLine);
            }
            LOGGER.Log(level, message, args);
        }

        private static Direction getOppositeDirection(Direction direction) {
            switch (direction) {
                case Direction.NORTH:
                    return Direction.SOUTH;
                case Direction.SOUTH:
                    return Direction.NORTH;
                case Direction.EAST:
                    return Direction.WEST;
                case Direction.WEST:
                    return Direction.EAST;
                default:
                    return Direction.UNKNOWN;
            }
        }

        public Form1() {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e) {
            txtBoxLog.Clear();
            this.RunImageFileAnalysis();
        }

        private void RunImageFileAnalysis() {
            this.imageAnalysisWorker = new BackgroundWorker();
            this.imageAnalysisWorker.DoWork += this.ImageAnalysisWorker_DoWork;
            this.imageAnalysisWorker.ProgressChanged += this.ImageAnalysisWorker_ProgressChanged;
            this.imageAnalysisWorker.RunWorkerCompleted += this.ImageAnalysisWorker_RunWorkerCompleted;
            this.imageAnalysisWorker.WorkerReportsProgress = true;
            this.imageAnalysisWorker.RunWorkerAsync();
        }

        private void ImageAnalysisWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            int counter = 0;
            int percentage = -1;
            string[] pieces = Directory.GetFiles(PIECE_DIRECTORY);
            int maxCounter = pieces.Length;

            // prepare edge maps
            this.mainCounterEdgeMap.Clear();
            this.mainCounterEdgeMap.Add(Direction.NORTH, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.mainCounterEdgeMap.Add(Direction.SOUTH, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.mainCounterEdgeMap.Add(Direction.EAST, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.mainCounterEdgeMap.Add(Direction.WEST, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.borderCounterEdgeMap.Clear();
            this.borderCounterEdgeMap.Add(Direction.NORTH, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.borderCounterEdgeMap.Add(Direction.SOUTH, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.borderCounterEdgeMap.Add(Direction.EAST, new Dictionary<byte, HashSet<PuzzlePiece>>());
            this.borderCounterEdgeMap.Add(Direction.WEST, new Dictionary<byte, HashSet<PuzzlePiece>>());

            for (byte i = 0; i <= PuzzlePiece.EDGE_MATCH_BYTE; i++) {
                this.mainCounterEdgeMap[Direction.NORTH].Add(i, new HashSet<PuzzlePiece>());
                this.mainCounterEdgeMap[Direction.SOUTH].Add(i, new HashSet<PuzzlePiece>());
                this.mainCounterEdgeMap[Direction.EAST].Add(i, new HashSet<PuzzlePiece>());
                this.mainCounterEdgeMap[Direction.WEST].Add(i, new HashSet<PuzzlePiece>());
                this.borderCounterEdgeMap[Direction.NORTH].Add(i, new HashSet<PuzzlePiece>());
                this.borderCounterEdgeMap[Direction.SOUTH].Add(i, new HashSet<PuzzlePiece>());
                this.borderCounterEdgeMap[Direction.EAST].Add(i, new HashSet<PuzzlePiece>());
                this.borderCounterEdgeMap[Direction.WEST].Add(i, new HashSet<PuzzlePiece>());
            }

            foreach (string fileLocation in pieces) {
                PuzzlePiece piece = new PuzzlePiece(fileLocation);
                this.pieceData.Add(piece);

                Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> edgeMap = (piece.PuzzleEdges.Count < 4) ? borderCounterEdgeMap : mainCounterEdgeMap;
                // fill edge maps
                if (!piece.PuzzleEdges.Contains(Direction.NORTH)) {
                    edgeMap[Direction.NORTH][PuzzlePiece.InvertEdge(piece.NorthEdge)].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(Direction.SOUTH)) {
                    edgeMap[Direction.SOUTH][PuzzlePiece.InvertEdge(piece.SouthEdge)].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(Direction.EAST)) {
                    edgeMap[Direction.EAST][PuzzlePiece.InvertEdge(piece.EastEdge)].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(Direction.WEST)) {
                    edgeMap[Direction.WEST][PuzzlePiece.InvertEdge(piece.WestEdge)].Add(piece);
                }

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
            this.LogWithBox(LogLevel.Info, "Completed {0}% of file read job.", e.ProgressPercentage);
        }

        private void ImageAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.LogWithBox(LogLevel.Info, "Initial analysis done!");

            foreach (KeyValuePair<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> direction in this.mainCounterEdgeMap) {
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
                    this.LogWithBox(LogLevel.Trace, "{0} {1} {2}", direction.Key, edge.Key, edgeCount);
                }
                this.LogWithBox(LogLevel.Info, "{0} {1}-{2}", direction.Key, dirMin, dirMax);
            }

            if (!this.pieceData.Any(x => x.PuzzleEdges.Count != 0)) {
                this.LogWithBox(LogLevel.Info, "Found NO edges or corners!");
            }

            this.LogWithBox(LogLevel.Info, "Running cross-image analysis.");
            this.RunBorderSolver();
        }

        private void RunBorderSolver() {
            this.borderSolverWorker = new BackgroundWorker();
            this.borderSolverWorker.DoWork += this.BorderSolverWorker_DoWork;
            this.borderSolverWorker.ProgressChanged += this.BorderSolverWorker_ProgressChanged;
            this.borderSolverWorker.RunWorkerCompleted += this.BorderSolverWorker_RunWorkerCompleted;
            this.borderSolverWorker.WorkerReportsProgress = true;
            this.borderSolverWorker.RunWorkerAsync();
        }

        private void BorderSolverWorker_DoWork(object sender, DoWorkEventArgs e) {
            HashSet<PuzzlePiece> borderPieces = this.pieceData.Where(x => x.PuzzleEdges.Count < 4).ToHashSet();
            //this.solve(borderPieces, borderCounterEdgeMap);
        }

        private void BorderSolverWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            this.LogWithBox(LogLevel.Debug, "Solving Border ... {0}%", e.ProgressPercentage);
        }

        private void BorderSolverWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            //this.RunPuzzleSolver();
        }

        private void RunPuzzleSolver() {
            this.puzzleSolverWorker = new BackgroundWorker();
            this.puzzleSolverWorker.DoWork += this.PuzzleSolverWorker_DoWork;
            this.puzzleSolverWorker.ProgressChanged += this.PuzzleSolverWorker_ProgressChanged;
            this.puzzleSolverWorker.RunWorkerCompleted += this.PuzzleSolverWorker_RunWorkerCompleted;
            this.puzzleSolverWorker.WorkerReportsProgress = true;
            this.puzzleSolverWorker.RunWorkerAsync(this.pieceData.Where(x => x.PuzzleEdges.Count == 4).ToHashSet());
        }

        private void PuzzleSolverWorker_DoWork(object sender, DoWorkEventArgs e) {
            //this.LogWithBox(LogLevel.Info, "Running border solver.");

            //HashSet<PuzzlePiece> availablePieces = e.Argument as HashSet<PuzzlePiece>;

            //HashSet<PuzzlePiece> usedPieces = new HashSet<PuzzlePiece>();
            //Dictionary<int, Dictionary<int, PuzzlePiece>> finishedSubPuzzle = new Dictionary<int, Dictionary<int, PuzzlePiece>>();
            //int addedPieces = int.MaxValue;
            ////if (this.masterPuzzle.Count == 0) {
            ////    //AddPieceToPuzzle(0, 0, this.pieceData.Find(x => x.PuzzleEdges.Contains(Direction.NORTH) && x.PuzzleEdges.Contains(Direction.EAST)), finishedSubPuzzle, usedPieces);
            ////    AddPieceToPuzzle(0, 0, this.pieceData.Find(x => x.Filename.EndsWith("3FED7F1.png")), finishedSubPuzzle, usedPieces);
            ////}
            ////else {
            //// pick a random start piece
            //AddPieceToPuzzle(0, 0, this.pieceData[this.prng.Next(this.pieceData.Count)], finishedSubPuzzle, usedPieces);
            ////}

            //while (addedPieces > 0) {
            //    addedPieces = 0;

            //    List<MissingPiece> missingNeighboringPieces = new List<MissingPiece>();
            //    // iterate all piece that are done
            //    foreach (KeyValuePair<int, Dictionary<int, PuzzlePiece>> finishedColumn in finishedSubPuzzle) {
            //        foreach (KeyValuePair<int, PuzzlePiece> finishedPiece in finishedColumn.Value) {
            //            // add every missing neighbor of the current piece
            //            foreach (Tuple<int, int> emptySpot in GetMissingNeighbors(finishedColumn.Key, finishedPiece.Key, finishedSubPuzzle)) {
            //                MissingPiece missingPiece = new MissingPiece {
            //                    X = emptySpot.Item1,
            //                    Y = emptySpot.Item2,
            //                    Edges = GetSurroundingEdges(emptySpot.Item1, emptySpot.Item2, finishedSubPuzzle)
            //                };
            //                // all physically matching pieces (that were not already used)
            //                IEnumerable<PuzzlePiece> physicalMatches = this.pieceData.Where(x =>
            //                    !usedPieces.Contains(x) &&
            //                    (!missingPiece.Edges.ContainsKey(Direction.NORTH) || this.mainCounterEdgeMap[Direction.NORTH][missingPiece.Edges[Direction.NORTH]].Contains(x)) &&
            //                    (!missingPiece.Edges.ContainsKey(Direction.SOUTH) || this.mainCounterEdgeMap[Direction.SOUTH][missingPiece.Edges[Direction.SOUTH]].Contains(x)) &&
            //                    (!missingPiece.Edges.ContainsKey(Direction.EAST) || this.mainCounterEdgeMap[Direction.EAST][missingPiece.Edges[Direction.EAST]].Contains(x)) &&
            //                    (!missingPiece.Edges.ContainsKey(Direction.WEST) || this.mainCounterEdgeMap[Direction.WEST][missingPiece.Edges[Direction.WEST]].Contains(x))
            //                );
            //                // plus their optical match
            //                List<Tuple<PuzzlePiece, int>> sortedMatches = new List<Tuple<PuzzlePiece, int>>();
            //                foreach (PuzzlePiece physicallyMatchingPiece in physicalMatches) {
            //                    int opticalDistance = 0;
            //                    if (missingPiece.Edges.ContainsKey(Direction.NORTH)) {
            //                        opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, finishedSubPuzzle[emptySpot.Item1][emptySpot.Item2 - 1], Direction.NORTH);
            //                    }
            //                    if (missingPiece.Edges.ContainsKey(Direction.SOUTH)) {
            //                        opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, finishedSubPuzzle[emptySpot.Item1][emptySpot.Item2 + 1], Direction.SOUTH);
            //                    }
            //                    if (missingPiece.Edges.ContainsKey(Direction.EAST)) {
            //                        opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, finishedSubPuzzle[emptySpot.Item1 + 1][emptySpot.Item2], Direction.EAST);
            //                    }
            //                    if (missingPiece.Edges.ContainsKey(Direction.WEST)) {
            //                        opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, finishedSubPuzzle[emptySpot.Item1 - 1][emptySpot.Item2], Direction.WEST);
            //                    }
            //                    sortedMatches.Add(new Tuple<PuzzlePiece, int>(physicallyMatchingPiece, opticalDistance));
            //                }
            //                // sort by optical difference
            //                missingPiece.MatchingPieces.AddRange(sortedMatches.OrderBy(x => x.Item2).ToList());
            //                missingNeighboringPieces.Add(missingPiece);
            //            }
            //        }
            //    }

            //    // add the good matches to the scene
            //    foreach (MissingPiece missing in missingNeighboringPieces) {
            //        if ((missing.MatchingPieces.Count == 0) && (usedPieces.Count != this.pieceData.Count)) {
            //            this.LogWithBox(LogLevel.Debug, "Couldn't find any more matching pieces.");
            //            e.Result = finishedSubPuzzle;
            //            return;
            //        }
            //        // either just one option or a good one was found
            //        if ((missing.MatchingPieces.Count == 1) || ((missing.MatchingPieces[0].Item2 <= missing.MatchingPieces[1].Item2 * DISTANCE_FACTOR_2ND_PLACE) && (missing.MatchingPieces[0].Item2 < DISTANCE_THRESHOLD))) {
            //            this.LogWithBox(LogLevel.Debug, "Added piece with distance of {0}.", missing.MatchingPieces[0].Item2);
            //            AddPieceToPuzzle(missing.X, missing.Y, missing.MatchingPieces[0].Item1, finishedSubPuzzle, usedPieces);
            //            addedPieces++;
            //        }
            //    }
            //    this.LogWithBox(LogLevel.Debug, "Added {0} pieces to the puzzle.", addedPieces);
            //}

            //this.LogWithBox(LogLevel.Debug, "Could not add anymore pieces to the puzzle :/");
            //e.Result = finishedSubPuzzle;
        }

        private void PuzzleSolverWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            this.LogWithBox(LogLevel.Debug, "Solving Main Puzzle ... {0}%", e.ProgressPercentage);
        }

        private void PuzzleSolverWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            //Dictionary<int, Dictionary<int, PuzzlePiece>> finishedPuzzle = e.Result as Dictionary<int, Dictionary<int, PuzzlePiece>>;

            //// too small
            //if ((finishedPuzzle == null) || (finishedPuzzle.Count < SUCCESS_MIN_PIECE_COUNT)) {
            //    this.LogWithBox(LogLevel.Debug, "Solution discarded because it was too small or invalid.");
            //}
            //else {
            //    successCount++;
            //    this.LogWithBox(LogLevel.Info, "Solved sub-puzzle #{0}.", successCount);

            //    Bitmap subImage = GenerateBitmapFromPuzzle(finishedPuzzle);
            //    picBoxSub.Image = subImage;

            //    if (this.TryAddPuzzleToMaster(finishedPuzzle)) {
            //        this.LogWithBox(LogLevel.Info, "Successfully added sub-puzzle to master!");
            //        subImage.Save(this.runId + "_" + successCount + "_sub.png", ImageFormat.Png);

            //        foreach (Dictionary<int, Dictionary<int, PuzzlePiece>> floatingSubPuzzle in this.freeFloatingSubPuzzles) {
            //            if (this.TryAddPuzzleToMaster(floatingSubPuzzle)) {
            //                this.LogWithBox(LogLevel.Info, "Successfully added floating sub-puzzle to master!");
            //            }
            //        }
            //        Bitmap mainImage = GenerateBitmapFromPuzzle(this.masterPuzzle);
            //        picBoxMaster.Image = mainImage;
            //        mainImage.Save(this.runId + "_" + successCount + "_master.png", ImageFormat.Png);
            //    }
            //    else {
            //        // put it away and try again later
            //        this.freeFloatingSubPuzzles.Add(finishedPuzzle);
            //        subImage.Save(this.runId + "_" + successCount + "_flo.png", ImageFormat.Png);
            //    }
            //}

            //if (this.successCount < SUCCESS_CUTOFF) {
            //    RunPuzzleSolver();
            //}
        }
    }
}
