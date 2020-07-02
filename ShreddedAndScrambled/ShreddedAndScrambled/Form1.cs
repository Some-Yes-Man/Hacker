using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ShreddedAndScrambled {
    public partial class Form1 : Form {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        //private void LogWithBox(LogLevel level, string message) {
        //    this.LogWithBox(level, message, null);
        //}

        private void LogWithBox(LogLevel level, string message, params object[] args) {
            if ((level == LogLevel.Info) || (level == LogLevel.Warn) || (level == LogLevel.Error)) {
                txtBoxLog.AppendText(string.Format(message, args) + Environment.NewLine);
            }
            LOGGER.Log(level, message, args);
        }

        //private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\ManyPieces";
        private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\Pieces";
        //private const string PIECE_DIRECTORY = @"E:\Yes-Man\Downloads\ManyPieces";
        //private const string PIECE_DIRECTORY = @"E:\Yes-Man\Downloads\Pieces";

        private const double DISTANCE_FACTOR_2ND_PLACE = 0.8;
        private const int DISTANCE_THRESHOLD = 1000;
        private const int SUCCESS_CUTOFF = 1000;
        private const int SUCCESS_MIN_PIECE_COUNT = 15;
        private const int MASTER_ADD_MIN_MATCH_COUNT = 5;

        private BackgroundWorker imageAnalysisWorker;
        private BackgroundWorker crossAnalysisWorker;
        private BackgroundWorker puzzleSolverWorker;

        private readonly List<PuzzlePiece> pieceData = new List<PuzzlePiece>();
        private readonly Dictionary<byte, HashSet<PuzzlePiece>> hueHistogram = new Dictionary<byte, HashSet<PuzzlePiece>>();
        private readonly Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> counterEdgeMap = new Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>>();
        private readonly HashSet<Dictionary<int, Dictionary<int, PuzzlePiece>>> freeFloatingSubPuzzles = new HashSet<Dictionary<int, Dictionary<int, PuzzlePiece>>>();
        private readonly Dictionary<int, Dictionary<int, PuzzlePiece>> masterPuzzle = new Dictionary<int, Dictionary<int, PuzzlePiece>>();
        private readonly HashSet<PuzzlePiece> piecesUsedInMaster = new HashSet<PuzzlePiece>();

        private readonly Random prng = new Random();
        private readonly string runId = Guid.NewGuid().ToString().Substring(0, 8);

        private int successCount = 0;

        public Form1() {
            InitializeComponent();
        }

        private void Run_Click(object sender, EventArgs e) {
            txtBoxLog.Clear();
            this.successCount = 0;

            if (this.pieceData.Count == 0) {
                this.RunImageFileAnalysis();
            }
            else {
                this.RunCrossImageAnalysis();
            }
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

            // prepare counter edge map
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
            // prepare histogram
            this.hueHistogram.Clear();
            for (int i = 0; i < 256; i++) {
                this.hueHistogram.Add((byte)i, new HashSet<PuzzlePiece>());
            }

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
            this.LogWithBox(LogLevel.Info, "Completed {0}% of file read job.", e.ProgressPercentage);
        }

        private void ImageAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.LogWithBox(LogLevel.Info, "Initial analysis done!");

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
                    this.LogWithBox(LogLevel.Trace, "{0} {1} {2}", direction.Key, edge.Key, edgeCount);
                }
                this.LogWithBox(LogLevel.Info, "{0} {1}-{2}", direction.Key, dirMin, dirMax);
            }

            if (!this.pieceData.Any(x => x.PuzzleEdges.Count != 0)) {
                this.LogWithBox(LogLevel.Info, "Found NO edges or corners!");
            }

            this.LogWithBox(LogLevel.Info, "Running cross-image analysis.");
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
            this.LogWithBox(LogLevel.Debug, "Completed {0}% of cross-image analysis.");
        }

        private void CrossAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.LogWithBox(LogLevel.Debug, "Cross-image analysis done.");

            this.LogWithBox(LogLevel.Debug, "Starting puzzle solver.");
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
            this.LogWithBox(LogLevel.Debug, "Running solver to find solution {0}.", this.successCount + 1);

            HashSet<PuzzlePiece> usedPieces = new HashSet<PuzzlePiece>();
            Dictionary<int, Dictionary<int, PuzzlePiece>> finishedSubPuzzle = new Dictionary<int, Dictionary<int, PuzzlePiece>>();
            int addedPieces = int.MaxValue;
            //if (this.masterPuzzle.Count == 0) {
            //    //AddPieceToPuzzle(0, 0, this.pieceData.Find(x => x.PuzzleEdges.Contains(Direction.NORTH) && x.PuzzleEdges.Contains(Direction.EAST)), finishedSubPuzzle, usedPieces);
            //    AddPieceToPuzzle(0, 0, this.pieceData.Find(x => x.Filename.EndsWith("3FED7F1.png")), finishedSubPuzzle, usedPieces);
            //}
            //else {
            // pick a random start piece
            AddPieceToPuzzle(0, 0, this.pieceData[this.prng.Next(this.pieceData.Count)], finishedSubPuzzle, usedPieces);
            //}

            while (addedPieces > 0) {
                addedPieces = 0;

                List<MissingPiece> missingNeighboringPieces = new List<MissingPiece>();
                // iterate all piece that are done
                foreach (KeyValuePair<int, Dictionary<int, PuzzlePiece>> finishedColumn in finishedSubPuzzle) {
                    foreach (KeyValuePair<int, PuzzlePiece> finishedPiece in finishedColumn.Value) {
                        // add every missing neighbor of the current piece
                        foreach (Tuple<int, int> emptySpot in GetMissingNeighbors(finishedColumn.Key, finishedPiece.Key, finishedSubPuzzle)) {
                            MissingPiece missingPiece = new MissingPiece {
                                X = emptySpot.Item1,
                                Y = emptySpot.Item2,
                                Edges = GetSurroundingEdges(emptySpot.Item1, emptySpot.Item2, finishedSubPuzzle)
                            };
                            // all physically matching pieces (that were not already used)
                            IEnumerable<PuzzlePiece> physicalMatches = this.pieceData.Where(x =>
                                !usedPieces.Contains(x) &&
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
                                    opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, finishedSubPuzzle[emptySpot.Item1][emptySpot.Item2 - 1], Direction.NORTH);
                                }
                                if (missingPiece.Edges.ContainsKey(Direction.SOUTH)) {
                                    opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, finishedSubPuzzle[emptySpot.Item1][emptySpot.Item2 + 1], Direction.SOUTH);
                                }
                                if (missingPiece.Edges.ContainsKey(Direction.EAST)) {
                                    opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, finishedSubPuzzle[emptySpot.Item1 + 1][emptySpot.Item2], Direction.EAST);
                                }
                                if (missingPiece.Edges.ContainsKey(Direction.WEST)) {
                                    opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, finishedSubPuzzle[emptySpot.Item1 - 1][emptySpot.Item2], Direction.WEST);
                                }
                                sortedMatches.Add(new Tuple<PuzzlePiece, int>(physicallyMatchingPiece, opticalDistance));
                            }
                            // sort by optical difference
                            missingPiece.MatchingPieces.AddRange(sortedMatches.OrderBy(x => x.Item2).ToList());
                            missingNeighboringPieces.Add(missingPiece);
                        }
                    }
                }

                // add the good matches to the scene
                foreach (MissingPiece missing in missingNeighboringPieces) {
                    if ((missing.MatchingPieces.Count == 0) && (usedPieces.Count != this.pieceData.Count)) {
                        this.LogWithBox(LogLevel.Debug, "Couldn't find any more matching pieces.");
                        e.Result = finishedSubPuzzle;
                        return;
                    }
                    // either just one option or a good one was found
                    if ((missing.MatchingPieces.Count == 1) || ((missing.MatchingPieces[0].Item2 <= missing.MatchingPieces[1].Item2 * DISTANCE_FACTOR_2ND_PLACE) && (missing.MatchingPieces[0].Item2 < DISTANCE_THRESHOLD))) {
                        this.LogWithBox(LogLevel.Debug, "Added piece with distance of {0}.", missing.MatchingPieces[0].Item2);
                        AddPieceToPuzzle(missing.X, missing.Y, missing.MatchingPieces[0].Item1, finishedSubPuzzle, usedPieces);
                        addedPieces++;
                    }
                }
                this.LogWithBox(LogLevel.Debug, "Added {0} pieces to the puzzle.", addedPieces);
            }

            this.LogWithBox(LogLevel.Debug, "Could not add anymore pieces to the puzzle :/");
            e.Result = finishedSubPuzzle;
        }

        private void PuzzleSolverWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            this.LogWithBox(LogLevel.Debug, "Solving ... {0}%", e.ProgressPercentage);
        }

        private void PuzzleSolverWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            Dictionary<int, Dictionary<int, PuzzlePiece>> finishedPuzzle = e.Result as Dictionary<int, Dictionary<int, PuzzlePiece>>;

            // too small
            if ((finishedPuzzle == null) || (finishedPuzzle.Count < SUCCESS_MIN_PIECE_COUNT)) {
                this.LogWithBox(LogLevel.Debug, "Solution discarded because it was too small or invalid.");
            }
            else {
                successCount++;
                this.LogWithBox(LogLevel.Info, "Solved sub-puzzle #{0}.", successCount);

                Bitmap subImage = GenerateBitmapFromPuzzle(finishedPuzzle);
                picBoxSub.Image = subImage;

                if (this.TryAddPuzzleToMaster(finishedPuzzle)) {
                    this.LogWithBox(LogLevel.Info, "Successfully added sub-puzzle to master!");
                    subImage.Save(this.runId + "_" + successCount + "_sub.png", ImageFormat.Png);

                    foreach (Dictionary<int, Dictionary<int, PuzzlePiece>> floatingSubPuzzle in this.freeFloatingSubPuzzles) {
                        if (this.TryAddPuzzleToMaster(floatingSubPuzzle)) {
                            this.LogWithBox(LogLevel.Info, "Successfully added floating sub-puzzle to master!");
                        }
                    }
                    Bitmap mainImage = GenerateBitmapFromPuzzle(this.masterPuzzle);
                    picBoxMaster.Image = mainImage;
                    mainImage.Save(this.runId + "_" + successCount + "_master.png", ImageFormat.Png);
                }
                else {
                    // put it away and try again later
                    this.freeFloatingSubPuzzles.Add(finishedPuzzle);
                    subImage.Save(this.runId + "_" + successCount + "_flo.png", ImageFormat.Png);
                }
            }

            if (this.successCount < SUCCESS_CUTOFF) {
                RunPuzzleSolver();
            }
        }

        private bool TryAddPuzzleToMaster(Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            // check whether the minimum overlap does exist
            IEnumerable<Tuple<int, int, PuzzlePiece>> masterPieces = this.masterPuzzle.SelectMany(x => x.Value.Select(y => new Tuple<int, int, PuzzlePiece>(x.Key, y.Key, y.Value)));
            bool masterEmpty = masterPieces.Count() == 0;
            IEnumerable<Tuple<int, int, PuzzlePiece>> subPieces = puzzle.SelectMany(x => x.Value.Select(y => new Tuple<int, int, PuzzlePiece>(x.Key, y.Key, y.Value)));
            // just pieces contained in both; offset is still unclear
            IEnumerable<Tuple<int, int, PuzzlePiece>> overlappingPieces = subPieces.Where(x => masterPieces.Any(y => (y.Item3.Id == x.Item3.Id)) || masterEmpty);

            // either the master is empty or a properly overlapping pieces has been found
            if (masterEmpty || (overlappingPieces.Count() >= MASTER_ADD_MIN_MATCH_COUNT)) {
                Dictionary<PuzzlePiece, Tuple<int, int>> masterCoordinates = new Dictionary<PuzzlePiece, Tuple<int, int>>(masterPieces.Select(x => new KeyValuePair<PuzzlePiece, Tuple<int, int>>(x.Item3, new Tuple<int, int>(x.Item1, x.Item2))));
                Dictionary<PuzzlePiece, Tuple<int, int>> subCoordinates = new Dictionary<PuzzlePiece, Tuple<int, int>>(subPieces.Select(x => new KeyValuePair<PuzzlePiece, Tuple<int, int>>(x.Item3, new Tuple<int, int>(x.Item1, x.Item2))));

                // give a single vote for NO offset (in case master is empty)
                Dictionary<Tuple<int, int>, int> offsetVotes = new Dictionary<Tuple<int, int>, int>();
                offsetVotes.Add(new Tuple<int, int>(0, 0), 1);

                // only if a master is already present, try to determine the offset
                if (!masterEmpty) {
                    foreach (Tuple<int, int, PuzzlePiece> overlap in overlappingPieces) {
                        Tuple<int, int> offset = new Tuple<int, int>(masterCoordinates[overlap.Item3].Item1 - subCoordinates[overlap.Item3].Item1, masterCoordinates[overlap.Item3].Item2 - subCoordinates[overlap.Item3].Item2);
                        if (!offsetVotes.ContainsKey(offset)) {
                            offsetVotes.Add(offset, 1);
                        }
                        else {
                            offsetVotes[offset]++;
                        }
                    }
                }
                List<int> votes = offsetVotes.Select(x => x.Value).ToList();
                votes.Sort((x, y) => -1 * x.CompareTo(y));

                // if the vote wasn't decisive, abort
                if ((votes.Count > 1) && (votes[0] < 2 * votes[1])) {
                    this.LogWithBox(LogLevel.Warn, "Offset of overlapping part could not be determined.");
                    return false;
                }

                // insert pieces from subpuzzle into master, using the offset; if differences are found, delete the position from master
                Tuple<int, int> properOffset = offsetVotes.First(x => x.Value == votes[0]).Key;
                foreach (Tuple<int, int, PuzzlePiece> piece in subPieces) {
                    // piece is new in master; add
                    if (!this.masterPuzzle.ContainsKey(piece.Item1 + properOffset.Item1) || !this.masterPuzzle[piece.Item1 + properOffset.Item1].ContainsKey(piece.Item2 + properOffset.Item2)) {
                        AddPieceToPuzzle(piece.Item1 + properOffset.Item1, piece.Item2 + properOffset.Item2, piece.Item3, this.masterPuzzle, this.piecesUsedInMaster);
                    }
                    // piece exists in master AND is different; delete from master
                    else {
                        if (this.masterPuzzle[piece.Item1 + properOffset.Item1][piece.Item2 + properOffset.Item2].Id != piece.Item3.Id) {
                            RemovePieceFromPuzzle(piece.Item1 + properOffset.Item1, piece.Item2 + properOffset.Item2, piece.Item3, this.masterPuzzle, this.piecesUsedInMaster);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private static Bitmap GenerateBitmapFromPuzzle(Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            // find normalized picture coordinates
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            foreach (KeyValuePair<int, Dictionary<int, PuzzlePiece>> finishedColumn in puzzle) {
                minX = Math.Min(minX, finishedColumn.Key);
                maxX = Math.Max(maxX, finishedColumn.Key);
                foreach (KeyValuePair<int, PuzzlePiece> finishedRow in finishedColumn.Value) {
                    minY = Math.Min(minY, finishedRow.Key);
                    maxY = Math.Max(maxY, finishedRow.Key);
                }
            }

            // create empty image (of correct size)
            Bitmap output = new Bitmap((maxX - minX + 1) * (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH,
                (maxY - minY + 1) * (PuzzlePiece.DATA_HEIGHT - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH);
            // fill in the pieces we solved
            foreach (KeyValuePair<int, Dictionary<int, PuzzlePiece>> col in puzzle) {
                int normX = col.Key - minX;
                foreach (KeyValuePair<int, PuzzlePiece> row in col.Value) {
                    int normY = row.Key - minY;
                    PuzzlePiece piece = row.Value;

                    for (int y = 0; y < PuzzlePiece.DATA_HEIGHT; y++) {
                        for (int x = 0; x < PuzzlePiece.DATA_WIDTH; x++) {
                            if (!piece.ImageData[x, y].IsEmpty) {
                                output.SetPixel(normX * (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + x, normY * (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + y, piece.ImageData[x, y]);
                            }
                        }
                    }
                }
            }

            return output;
        }

        private static void AddPieceToPuzzle(int x, int y, PuzzlePiece piece, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle, HashSet<PuzzlePiece> usedPieces) {
            // if the algorithms solves the same pieces from two different directions at once...
            if (usedPieces.Contains(piece)) {
                return;
            }

            if (!puzzle.ContainsKey(x)) {
                puzzle.Add(x, new Dictionary<int, PuzzlePiece>());
            }
            if (!puzzle[x].ContainsKey(y)) {
                usedPieces.Add(piece);
                puzzle[x].Add(y, piece);
            }
            else {
                throw new InvalidOperationException("There is already a puzzle piece at position " + x + ":" + y);
            }
        }

        private static void RemovePieceFromPuzzle(int x, int y, PuzzlePiece piece, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle, HashSet<PuzzlePiece> usedPieces) {
            // just checking ... see above
            if (!usedPieces.Contains(piece)) {
                return;
            }

            puzzle[x].Remove(y);
            if (puzzle[x].Count == 0) {
                puzzle.Remove(x);
            }
            usedPieces.Remove(piece);
        }

        private static HashSet<Tuple<int, int>> GetMissingNeighbors(int x, int y, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            if (!puzzle.ContainsKey(x) || !puzzle[x].ContainsKey(y)) {
                throw new InvalidOperationException("There is no piece at the given position.");
            }
            PuzzlePiece piece = puzzle[x][y];
            HashSet<Tuple<int, int>> neighbors = new HashSet<Tuple<int, int>>();
            // north
            if (!piece.PuzzleEdges.Contains(Direction.NORTH) && (!puzzle.ContainsKey(x) || !puzzle[x].ContainsKey(y - 1))) {
                neighbors.Add(new Tuple<int, int>(x, y - 1));
            }
            // south
            if (!piece.PuzzleEdges.Contains(Direction.SOUTH) && (!puzzle.ContainsKey(x) || !puzzle[x].ContainsKey(y + 1))) {
                neighbors.Add(new Tuple<int, int>(x, y + 1));
            }
            // east
            if (!piece.PuzzleEdges.Contains(Direction.EAST) && (!puzzle.ContainsKey(x + 1) || !puzzle[x + 1].ContainsKey(y))) {
                neighbors.Add(new Tuple<int, int>(x + 1, y));
            }
            // west
            if (!piece.PuzzleEdges.Contains(Direction.WEST) && (!puzzle.ContainsKey(x - 1) || !puzzle[x - 1].ContainsKey(y))) {
                neighbors.Add(new Tuple<int, int>(x - 1, y));
            }
            return neighbors;
        }

        private static Dictionary<Direction, byte> GetSurroundingEdges(int x, int y, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            Dictionary<Direction, byte> surroundingEdges = new Dictionary<Direction, byte>();

            if (puzzle.ContainsKey(x) && puzzle[x].ContainsKey(y - 1)) {
                surroundingEdges.Add(Direction.NORTH, puzzle[x][y - 1].SouthEdge);
            }
            if (puzzle.ContainsKey(x) && puzzle[x].ContainsKey(y + 1)) {
                surroundingEdges.Add(Direction.SOUTH, puzzle[x][y + 1].NorthEdge);
            }
            if (puzzle.ContainsKey(x + 1) && puzzle[x + 1].ContainsKey(y)) {
                surroundingEdges.Add(Direction.EAST, puzzle[x + 1][y].WestEdge);
            }
            if (puzzle.ContainsKey(x - 1) && puzzle[x - 1].ContainsKey(y)) {
                surroundingEdges.Add(Direction.WEST, puzzle[x - 1][y].EastEdge);
            }

            return surroundingEdges;
        }

        public override string ToString() {
            return "foo";
        }

    }
}
