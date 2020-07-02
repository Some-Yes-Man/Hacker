using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace ShreddedAndScrambled {
    public partial class Form1 : Form {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private void LogWithBox(LogLevel level, string message, params object[] args) {
            if ((level == LogLevel.Info) || (level == LogLevel.Warn) || (level == LogLevel.Error)) {
                txtBoxLog.AppendText(string.Format(message, args) + Environment.NewLine);
            }
            LOGGER.Log(level, message, args);
        }

        //private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\ManyPieces";
        //private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\Pieces";
        //private const string PIECE_DIRECTORY = @"E:\Yes-Man\Downloads\ManyPieces";
        private const string PIECE_DIRECTORY = @"E:\Yes-Man\Downloads\Pieces";

        private const double DISTANCE_FACTOR_2ND_PLACE = 1.5;
        private const int DISTANCE_THRESHOLD = 1000;
        private const int MASTER_ADD_MIN_MATCH_COUNT = 5;

        private BackgroundWorker imageAnalysisWorker;
        private BackgroundWorker crossAnalysisWorker;
        private BackgroundWorker puzzleSolverWorker;

        private readonly List<PuzzlePiece> pieceData = new List<PuzzlePiece>();
        private readonly Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> counterEdgeMap = new Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>>();
        private readonly HashSet<Dictionary<int, Dictionary<int, PuzzlePiece>>> freeFloatingSubPuzzles = new HashSet<Dictionary<int, Dictionary<int, PuzzlePiece>>>();
        private readonly Dictionary<int, Dictionary<int, PuzzlePiece>> masterPuzzle = new Dictionary<int, Dictionary<int, PuzzlePiece>>();
        private readonly HashSet<PuzzlePiece> piecesUsedInMaster = new HashSet<PuzzlePiece>();

        private readonly Random prng = new Random();

        private string runId;

        public Form1() {
            InitializeComponent();
        }

        public override string ToString() {
            return "";
        }

        private void Run_Click(object sender, EventArgs e) {
            txtBoxLog.Clear();
            this.runId = Guid.NewGuid().ToString().Substring(0, 8);

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
                    LOGGER.Trace("{dir} {edge} {count}", direction.Key, edge.Key, edgeCount);
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
        }

        private void CrossAnalysisWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            LOGGER.Debug("Completed {percentage}% of cross-image analysis.", e.ProgressPercentage);
        }

        private void CrossAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            LOGGER.Debug("Cross-image analysis done.");
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
            AddPieceToPuzzle(0, 0, this.pieceData.Find(x => x.PuzzleEdges.Contains(Direction.NORTH) && x.PuzzleEdges.Contains(Direction.WEST)), this.masterPuzzle, this.piecesUsedInMaster);
            //AddPieceToPuzzle(0, 0, this.pieceData.Find(x => x.Filename.EndsWith("3FED7F1.png")), finishedSubPuzzle, usedPieces);
            //AddPieceToPuzzle(0, 0, this.pieceData[this.prng.Next(this.pieceData.Count)], finishedSubPuzzle, usedPieces);

            int addedPieces = int.MaxValue;

            while (addedPieces > 0) {
                addedPieces = 0;

                List<MissingPiece> missingNeighboringPieces = new List<MissingPiece>();
                // iterate all piece that are done
                foreach (KeyValuePair<int, Dictionary<int, PuzzlePiece>> finishedColumn in this.masterPuzzle) {
                    foreach (KeyValuePair<int, PuzzlePiece> finishedPiece in finishedColumn.Value) {
                        // add every missing neighbor of the current piece
                        foreach (Tuple<int, int> emptySpot in GetMissingNeighbors(finishedColumn.Key, finishedPiece.Key, this.masterPuzzle)) {
                            // skip if possible
                            if (missingNeighboringPieces.Any(x => (x.X == emptySpot.Item1) && (x.Y == emptySpot.Item2))) {
                                LOGGER.Debug("Skipped duplicate missing neighbor.");
                                break;
                            }

                            MissingPiece missingPiece = new MissingPiece {
                                X = emptySpot.Item1,
                                Y = emptySpot.Item2,
                                Edges = GetSurroundingEdges(emptySpot.Item1, emptySpot.Item2, this.masterPuzzle)
                            };
                            // all physically matching pieces (that were not already used)
                            IEnumerable<PuzzlePiece> physicalMatches = this.pieceData.Where(x =>
                                !this.piecesUsedInMaster.Contains(x) &&
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
                                    opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, this.masterPuzzle[emptySpot.Item1][emptySpot.Item2 - 1], Direction.NORTH);
                                }
                                if (missingPiece.Edges.ContainsKey(Direction.SOUTH)) {
                                    opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, this.masterPuzzle[emptySpot.Item1][emptySpot.Item2 + 1], Direction.SOUTH);
                                }
                                if (missingPiece.Edges.ContainsKey(Direction.EAST)) {
                                    opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, this.masterPuzzle[emptySpot.Item1 + 1][emptySpot.Item2], Direction.EAST);
                                }
                                if (missingPiece.Edges.ContainsKey(Direction.WEST)) {
                                    opticalDistance += PuzzlePiece.GetPieceEdgeDistance(physicallyMatchingPiece, this.masterPuzzle[emptySpot.Item1 - 1][emptySpot.Item2], Direction.WEST);
                                }
                                sortedMatches.Add(new Tuple<PuzzlePiece, int>(physicallyMatchingPiece, opticalDistance));
                            }
                            // sort by optical difference
                            missingPiece.MatchingPieces.AddRange(sortedMatches.OrderBy(x => x.Item2).ToList());
                            missingNeighboringPieces.Add(missingPiece);

                            // some feedback
                            StringBuilder builder = new StringBuilder();
                            int matchCount = missingPiece.MatchingPieces.Count;
                            if (matchCount == 0) {
                                builder.Append("Found NO matching piece for empty spot ").Append(emptySpot.Item1).Append(':').Append(emptySpot.Item2).Append('.');
                            }
                            else {
                                for (int i = 0; i < 3; i++) {
                                    if (matchCount > i) {
                                        builder.Append('D').Append(i).Append(':').Append(missingPiece.MatchingPieces[i].Item2).Append(' ');
                                    }
                                }
                                builder.Append("D~").Append(':').Append(missingPiece.MatchingPieces.Average(x => x.Item2)).Append(' ');
                            }
                            if (matchCount > 3) {
                                builder.Append("Dx").Append(':').Append(missingPiece.MatchingPieces[matchCount - 1].Item2).Append(' ');
                            }
                            LOGGER.Debug(builder.ToString());
                        }
                    }
                }

                // add the good matches to the scene
                foreach (MissingPiece missing in missingNeighboringPieces) {
                    if ((missing.MatchingPieces.Count == 0) && (this.piecesUsedInMaster.Count != this.pieceData.Count)) {
                        LOGGER.Debug("Couldn't find any more matching pieces.");
                        return;
                    }
                    // either just one option or a good one was found
                    if ((missing.MatchingPieces.Count == 1) || ((missing.MatchingPieces[0].Item2 * DISTANCE_FACTOR_2ND_PLACE <= missing.MatchingPieces[1].Item2) && (missing.MatchingPieces[0].Item2 < DISTANCE_THRESHOLD))) {
                        LOGGER.Debug("Added piece with distance of {dist}.", missing.MatchingPieces[0].Item2);
                        AddPieceToPuzzle(missing.X, missing.Y, missing.MatchingPieces[0].Item1, this.masterPuzzle, this.piecesUsedInMaster);
                        addedPieces++;
                    }
                }
                LOGGER.Debug("Added {count} pieces to the puzzle.", addedPieces);
            }

            LOGGER.Debug("Could not add anymore pieces to the puzzle :/");
        }

        private void PuzzleSolverWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            LOGGER.Debug("Solving ... {percentage}%", e.ProgressPercentage);
        }

        private void PuzzleSolverWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            PicBoxMaster.Image = GenerateBitmapFromPuzzle(this.masterPuzzle);
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

        private static PuzzlePiece DeterminePieceFromBitmapCoordinate(Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle, int imageX, int imageY) {
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

            // sanity check
            if ((imageX >= (maxX - minX + 1) * (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH)
                || (imageY >= (maxY - minY + 1) * (PuzzlePiece.DATA_HEIGHT - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH)) {
                throw new ArgumentException("Specified coordinates not inside actual image.");
            }

            int normPuzzleX = (imageX - PuzzlePiece.BIT_LENGTH) / (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + minX;
            int normPuzzleY = (imageY - PuzzlePiece.BIT_LENGTH) / (PuzzlePiece.DATA_HEIGHT - 2 * PuzzlePiece.BIT_LENGTH) + minY;

            return (!puzzle.ContainsKey(normPuzzleX) || !puzzle[normPuzzleX].ContainsKey(normPuzzleY)) ? null : puzzle[normPuzzleX][normPuzzleY];
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
            puzzle[x].Remove(y);
            usedPieces.Remove(piece);
        }

        private static void RemovePieceFromPuzzle(PuzzlePiece piece, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle, HashSet<PuzzlePiece> usedPieces) {
            KeyValuePair<int, Dictionary<int, PuzzlePiece>> targetColumn = puzzle.First(x => x.Value.Any(y => y.Value.Id == piece.Id));
            int targetY = targetColumn.Value.First(x => x.Value.Id == piece.Id).Key;
            puzzle[targetColumn.Key].Remove(targetY);
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

        private void PicBoxMaster_Click(object sender, EventArgs e) {
            PictureBox pictureBox = sender as PictureBox;
            MouseEventArgs mouseEventArgs = e as MouseEventArgs;

            // determine coordinates in picture
            txtBoxLog.AppendText("PicBoxRel " + mouseEventArgs.Location.ToString() + Environment.NewLine);
            Point imageCoordinates = GetImageCoordinatesFromPictureBoxClick(pictureBox, mouseEventArgs.X, mouseEventArgs.Y);
            txtBoxLog.AppendText("ImgRel " + (Point.Empty.Equals(imageCoordinates) ? "N/A" : imageCoordinates.ToString()) + Environment.NewLine);

            // calculate piece in picture
            PuzzlePiece clickedPiece = DeterminePieceFromBitmapCoordinate(this.masterPuzzle, imageCoordinates.X, imageCoordinates.Y);
            txtBoxLog.AppendText("Piece #" + clickedPiece + Environment.NewLine);

            // right-click remove piece
            if ((clickedPiece != null) && (mouseEventArgs.Button == MouseButtons.Right)) {
                RemovePieceFromPuzzle(clickedPiece, this.masterPuzzle, this.piecesUsedInMaster);
                pictureBox.Image = GenerateBitmapFromPuzzle(this.masterPuzzle);
            }
        }

        private static Point GetImageCoordinatesFromPictureBoxClick(PictureBox pictureBox, int xBoxRelative, int yBoxRelative) {
            if ((pictureBox == null) || (pictureBox.Image == null)) {
                return Point.Empty;
            }

            int boxWidth = pictureBox.Width;
            int boxHeight = pictureBox.Height;
            int imgWidth = pictureBox.Image.Width;
            int imgHeight = pictureBox.Image.Height;

            Point possibleImageLocation;

            switch (pictureBox.SizeMode) {
                // image is centered and either padded or cropped equally
                case PictureBoxSizeMode.CenterImage:
                    possibleImageLocation = new Point(xBoxRelative - (boxWidth - imgWidth) / 2, yBoxRelative - (boxHeight - imgHeight) / 2);
                    break;
                // both axis are stretched to fit the picture box independently
                case PictureBoxSizeMode.StretchImage:
                    possibleImageLocation = new Point((int)((double)xBoxRelative * imgWidth / boxWidth), (int)((double)yBoxRelative * imgHeight / boxHeight));
                    break;
                // both axis are strechted by the same factor, depending on which one is the limiting dimension
                case PictureBoxSizeMode.Zoom:
                    // compare aspect ratios
                    if ((double)imgWidth / imgHeight > (double)boxWidth / boxHeight) {
                        // limited by width; so width is easy, height is NOT
                        double scale = (double)boxWidth / imgWidth;
                        possibleImageLocation = new Point((int)((double)xBoxRelative * imgWidth / boxWidth), (int)((yBoxRelative - (boxHeight - scale * imgHeight) / 2) / scale));
                    }
                    else {
                        // limited by height; so height is easy, while width is NOT
                        double scale = (double)boxHeight / imgHeight;
                        possibleImageLocation = new Point((int)((xBoxRelative - (boxWidth - scale * imgWidth) / 2) / scale), (int)((double)yBoxRelative * imgHeight / boxHeight));
                    }
                    break;
                // image is displayed in the upper left corner and not streched (in auto the control will try to adapt)
                case PictureBoxSizeMode.Normal:
                case PictureBoxSizeMode.AutoSize:
                    possibleImageLocation = new Point(xBoxRelative, yBoxRelative);
                    break;
                default:
                    possibleImageLocation = Point.Empty;
                    break;
            }

            if ((possibleImageLocation.X >= 0) && (possibleImageLocation.X < imgWidth) && (possibleImageLocation.Y >= 0) && (possibleImageLocation.Y < imgHeight)) {
                return possibleImageLocation;
            }
            else {
                return Point.Empty;
            }
        }

    }
}
