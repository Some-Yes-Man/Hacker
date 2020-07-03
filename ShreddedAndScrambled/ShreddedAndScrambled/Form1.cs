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
                if (txtBoxLog.InvokeRequired) {
                    txtBoxLog.Invoke(new MethodInvoker(delegate { txtBoxLog.AppendText(string.Format(message, args) + Environment.NewLine); }));
                }
                else {
                    txtBoxLog.AppendText(string.Format(message, args) + Environment.NewLine);
                }
            }
            LOGGER.Log(level, message, args);
        }

        //private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\ManyPieces";
        private const string PIECE_DIRECTORY = @"C:\Users\rkrausse\Downloads\Pieces";
        //private const string PIECE_DIRECTORY = @"E:\Yes-Man\Downloads\ManyPieces";
        //private const string PIECE_DIRECTORY = @"E:\Yes-Man\Downloads\Pieces";

        private const double DISTANCE_FACTOR_2ND_PLACE = 2;
        private const double DISTANCE_TOP_FACTOR = 0.02;
        private const int IMAGE_LIST_ZOOM_FACTOR = 4;
        private const int RENDER_PADDING = 1;

        private static readonly Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> COUNTER_EDGE_MAP = new Dictionary<Direction, Dictionary<byte, HashSet<PuzzlePiece>>>();
        private static readonly List<PuzzlePiece> PIECE_DATA = new List<PuzzlePiece>();
        private static readonly object masterLock = new object();

        private readonly Dictionary<int, Dictionary<int, PuzzlePiece>> masterPuzzle = new Dictionary<int, Dictionary<int, PuzzlePiece>>();
        private readonly HashSet<PuzzlePiece> piecesUsedInMaster = new HashSet<PuzzlePiece>();
        private readonly ImageList pieceSelectionImageList = new ImageList();

        private BackgroundWorker imageAnalysisWorker;
        private BackgroundWorker crossAnalysisWorker;
        private BackgroundWorker puzzleSolverWorker;

        private Point puzzleSelectionCoords;
        private bool puzzleSelectionEmpty;
        private bool firstRun = true;
        private Random prng = new Random();
        private int dynamicDistanceThreshold = int.MaxValue;
        private Point deleteMouseDown;
        private Point deleteMouseUp;
        private bool interruptSolver = false;

        public Form1() {
            InitializeComponent();

            this.pieceSelectionImageList.ColorDepth = ColorDepth.Depth24Bit;
            this.pieceSelectionImageList.ImageSize = new Size(PuzzlePiece.DATA_WIDTH * IMAGE_LIST_ZOOM_FACTOR, PuzzlePiece.DATA_HEIGHT * IMAGE_LIST_ZOOM_FACTOR);
            this.pieceSelectionImageList.TransparentColor = Color.Empty;

            this.ListViewPieceSelection.LargeImageList = this.pieceSelectionImageList;
        }

        public override string ToString() {
            return "";
        }

        private void Run_Click(object sender, EventArgs e) {
            this.interruptSolver = false;

            if (PIECE_DATA.Count == 0) {
                this.RunImageFileAnalysis();
            }
            else {
                this.RunPuzzleSolver();
            }
        }

        private void BtnReset_Click(object sender, EventArgs e) {
            this.masterPuzzle.Clear();
            this.piecesUsedInMaster.Clear();
            this.firstRun = true;
            this.interruptSolver = false;
            this.RunPuzzleSolver();
        }

        // initial analysis

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

            // prepare counter edge map
            COUNTER_EDGE_MAP.Clear();
            COUNTER_EDGE_MAP.Add(Direction.NORTH, new Dictionary<byte, HashSet<PuzzlePiece>>());
            COUNTER_EDGE_MAP.Add(Direction.SOUTH, new Dictionary<byte, HashSet<PuzzlePiece>>());
            COUNTER_EDGE_MAP.Add(Direction.EAST, new Dictionary<byte, HashSet<PuzzlePiece>>());
            COUNTER_EDGE_MAP.Add(Direction.WEST, new Dictionary<byte, HashSet<PuzzlePiece>>());
            for (byte i = 0; i <= PuzzlePiece.EDGE_MATCH_BYTE; i++) {
                COUNTER_EDGE_MAP[Direction.NORTH].Add(i, new HashSet<PuzzlePiece>());
                COUNTER_EDGE_MAP[Direction.SOUTH].Add(i, new HashSet<PuzzlePiece>());
                COUNTER_EDGE_MAP[Direction.EAST].Add(i, new HashSet<PuzzlePiece>());
                COUNTER_EDGE_MAP[Direction.WEST].Add(i, new HashSet<PuzzlePiece>());
            }

            foreach (string fileLocation in pieces) {
                PuzzlePiece piece = new PuzzlePiece(fileLocation);
                PIECE_DATA.Add(piece);

                // fill edge map
                if (!piece.PuzzleEdges.Contains(Direction.NORTH)) {
                    COUNTER_EDGE_MAP[Direction.NORTH][PuzzlePiece.InvertEdge(piece.NorthEdge)].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(Direction.SOUTH)) {
                    COUNTER_EDGE_MAP[Direction.SOUTH][PuzzlePiece.InvertEdge(piece.SouthEdge)].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(Direction.EAST)) {
                    COUNTER_EDGE_MAP[Direction.EAST][PuzzlePiece.InvertEdge(piece.EastEdge)].Add(piece);
                }
                if (!piece.PuzzleEdges.Contains(Direction.WEST)) {
                    COUNTER_EDGE_MAP[Direction.WEST][PuzzlePiece.InvertEdge(piece.WestEdge)].Add(piece);
                }

                counter++;
                if (Math.Floor((double)counter * 100 / maxCounter) > percentage) {
                    if (percentage % 10 == 0) {
                        worker.ReportProgress(percentage);
                    }
                    percentage++;
                }
            }
        }

        private void ImageAnalysisWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            string msg = "Completed " + e.ProgressPercentage + "% of file read job.";
            LOGGER.Info(msg);
            txtBoxLog.AppendText(msg + Environment.NewLine);
        }

        private void ImageAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.LogWithBox(LogLevel.Info, "Initial analysis done!");

            foreach (KeyValuePair<Direction, Dictionary<byte, HashSet<PuzzlePiece>>> direction in COUNTER_EDGE_MAP) {
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

            if (!PIECE_DATA.Any(x => x.PuzzleEdges.Count != 0)) {
                this.LogWithBox(LogLevel.Info, "Found NO edges or corners!");
            }

            this.LogWithBox(LogLevel.Info, "Running cross-image analysis.");
            this.RunCrossImageAnalysis();
        }

        // advanced analysis

        private void RunCrossImageAnalysis() {
            this.crossAnalysisWorker = new BackgroundWorker();
            this.crossAnalysisWorker.DoWork += this.CrossAnalysisWorker_DoWork;
            this.crossAnalysisWorker.ProgressChanged += this.CrossAnalysisWorker_ProgressChanged;
            this.crossAnalysisWorker.RunWorkerCompleted += this.CrossAnalysisWorker_RunWorkerCompleted;
            this.crossAnalysisWorker.WorkerReportsProgress = true;
            this.crossAnalysisWorker.RunWorkerAsync();
        }

        private void CrossAnalysisWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            // analyse all edges in order to find sensible thresholds
            int overallDistances = 0;
            int numberOfDonePieces = 0;
            int numberOfPieces = PIECE_DATA.Count;
            int percentageDone = -1;
            HashSet<int> masterDistances = new HashSet<int>();

            foreach (PuzzlePiece piece in PIECE_DATA) {
                HashSet<int> pieceDistances = new HashSet<int>();
                // north
                if (!piece.PuzzleEdges.Contains(Direction.NORTH)) {
                    foreach (PuzzlePiece counterPart in COUNTER_EDGE_MAP[Direction.SOUTH][piece.NorthEdge]) {
                        pieceDistances.Add(PuzzlePiece.GetPieceEdgeDistance(piece, counterPart, Direction.NORTH));
                    }
                }
                // south
                if (!piece.PuzzleEdges.Contains(Direction.SOUTH)) {
                    foreach (PuzzlePiece counterPart in COUNTER_EDGE_MAP[Direction.NORTH][piece.SouthEdge]) {
                        pieceDistances.Add(PuzzlePiece.GetPieceEdgeDistance(piece, counterPart, Direction.SOUTH));
                    }
                }
                // east
                if (!piece.PuzzleEdges.Contains(Direction.EAST)) {
                    foreach (PuzzlePiece counterPart in COUNTER_EDGE_MAP[Direction.WEST][piece.EastEdge]) {
                        pieceDistances.Add(PuzzlePiece.GetPieceEdgeDistance(piece, counterPart, Direction.EAST));
                    }
                }
                // west
                if (!piece.PuzzleEdges.Contains(Direction.WEST)) {
                    foreach (PuzzlePiece counterPart in COUNTER_EDGE_MAP[Direction.EAST][piece.WestEdge]) {
                        pieceDistances.Add(PuzzlePiece.GetPieceEdgeDistance(piece, counterPart, Direction.WEST));
                    }
                }
                overallDistances += pieceDistances.Count;
                int maxTopDistance = pieceDistances.OrderBy(x => x).Take((int)(DISTANCE_TOP_FACTOR * pieceDistances.Count)).Last();
                masterDistances.Add(maxTopDistance);

                numberOfDonePieces++;
                if (Math.Floor((double)numberOfDonePieces * 100 / numberOfPieces) > percentageDone) {
                    if (percentageDone % 10 == 0) {
                        worker.ReportProgress(percentageDone);
                    }
                    percentageDone++;
                }
            }

            int dynamicDistance = (int)masterDistances.Average();
            this.LogWithBox(LogLevel.Info, "Calculated a dynamic distance of {0} (using {1} individual piece distances from {2} pieces).", dynamicDistance, overallDistances, numberOfPieces);
        }

        private void CrossAnalysisWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            this.LogWithBox(LogLevel.Info, "Completed {0}% of cross-image analysis.", e.ProgressPercentage);
        }

        private void CrossAnalysisWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            LOGGER.Debug("Cross-image analysis done.");
            this.RunPuzzleSolver();
        }

        // solving the puzzle

        private void RunPuzzleSolver() {
            this.puzzleSolverWorker = new BackgroundWorker();
            this.puzzleSolverWorker.DoWork += this.PuzzleSolverWorker_DoWork;
            this.puzzleSolverWorker.ProgressChanged += this.PuzzleSolverWorker_ProgressChanged;
            this.puzzleSolverWorker.RunWorkerCompleted += this.PuzzleSolverWorker_RunWorkerCompleted;
            this.puzzleSolverWorker.WorkerReportsProgress = true;
            this.puzzleSolverWorker.RunWorkerAsync();
        }

        private void PuzzleSolverWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (this.firstRun) {
                //AddPieceToPuzzle(0, 0, PIECE_DATA.Find(x => x.PuzzleEdges.Contains(Direction.NORTH) && x.PuzzleEdges.Contains(Direction.WEST)), this.masterPuzzle, this.piecesUsedInMaster);
                //AddPieceToPuzzle(0, 0, PIECE_DATA.Find(x => x.Filename.EndsWith("52974AF.png")), this.masterPuzzle, this.piecesUsedInMaster);
                AddPieceToPuzzle(0, 0, PIECE_DATA.Find(x => x.Filename.EndsWith("B678A14.png")), this.masterPuzzle, this.piecesUsedInMaster);
                //AddPieceToPuzzle(0, 0, PIECE_DATA[this.prng.Next(PIECE_DATA.Count)], this.masterPuzzle, this.piecesUsedInMaster);
                this.firstRun = false;
            }

            int addedPieces = int.MaxValue;

            while ((addedPieces > 0) && !interruptSolver) {
                addedPieces = 0;

                List<MissingPiece> missingNeighboringPieces = new List<MissingPiece>();
                // iterate all piece that are done
                foreach (KeyValuePair<int, Dictionary<int, PuzzlePiece>> finishedColumn in this.masterPuzzle) {
                    if (this.interruptSolver) {
                        break;
                    }
                    foreach (KeyValuePair<int, PuzzlePiece> finishedPiece in finishedColumn.Value) {
                        if (this.interruptSolver) {
                            break;
                        }
                        // add every missing neighbor of the current piece
                        foreach (Tuple<int, int> emptySpot in GetMissingNeighbors(finishedColumn.Key, finishedPiece.Key, this.masterPuzzle)) {
                            // skip if possible
                            if (missingNeighboringPieces.Any(x => (x.X == emptySpot.Item1) && (x.Y == emptySpot.Item2))) {
                                LOGGER.Debug("Skipped duplicate missing neighbor.");
                                continue;
                            }

                            MissingPiece missingPiece = new MissingPiece {
                                X = emptySpot.Item1,
                                Y = emptySpot.Item2,
                                Edges = new Dictionary<Direction, byte>(GetSurroundingPieces(emptySpot.Item1, emptySpot.Item2, this.masterPuzzle).Select(x => new KeyValuePair<Direction, byte>(x.Key, GetCounterEdgeFromPiece(x.Value, x.Key))))
                            };
                            // all physically matching pieces (that were not already used) plus their optical match
                            List<Tuple<PuzzlePiece, int>> ratedFittingPieces = new List<Tuple<PuzzlePiece, int>>();
                            foreach (PuzzlePiece physicallyMatchingPiece in GetPhysicallyFittingPieces(missingPiece.Edges, PIECE_DATA, this.piecesUsedInMaster)) {
                                ratedFittingPieces.Add(new Tuple<PuzzlePiece, int>(physicallyMatchingPiece, PuzzlePiece.GetPieceMultiEdgeDistance(physicallyMatchingPiece, GetSurroundingPieces(emptySpot.Item1, emptySpot.Item2, this.masterPuzzle))));
                            }
                            // sort by optical difference
                            missingPiece.MatchingPieces.AddRange(ratedFittingPieces.OrderBy(x => x.Item2).ToList());
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
                foreach (MissingPiece missing in missingNeighboringPieces.OrderBy(x => x.Edges.Count)) {
                    if ((missing.MatchingPieces.Count == 0) && (this.piecesUsedInMaster.Count != PIECE_DATA.Count)) {
                        LOGGER.Debug("Couldn't find any more matching pieces.");
                        continue;
                    }
                    // either just one option or a good one was found
                    if ((missing.MatchingPieces.Count == 1) || ((missing.MatchingPieces[0].Item2 * DISTANCE_FACTOR_2ND_PLACE <= missing.MatchingPieces[1].Item2) && (missing.MatchingPieces[0].Item2 < this.dynamicDistanceThreshold))) {
                        LOGGER.Debug("Added piece with distance of {dist}.", missing.MatchingPieces[0].Item2);
                        AddPieceToPuzzle(missing.X, missing.Y, missing.MatchingPieces[0].Item1, this.masterPuzzle, this.piecesUsedInMaster);
                        addedPieces++;
                    }
                }

                worker.ReportProgress(0);

                LOGGER.Debug("Added {count} pieces to the puzzle.", addedPieces);
            }

            this.interruptSolver = false;

            LOGGER.Debug("Could not add anymore pieces to the puzzle :/");
        }

        private void PuzzleSolverWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            this.UpdateMasterView();
        }

        private void PuzzleSolverWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.UpdateMasterView();
        }

        // master modification

        private static void AddPieceToPuzzle(int puzzleX, int puzzleY, PuzzlePiece piece, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle, HashSet<PuzzlePiece> usedPieces) {
            lock (masterLock) {
                if (!puzzle.ContainsKey(puzzleX)) {
                    puzzle.Add(puzzleX, new Dictionary<int, PuzzlePiece>());
                }
                if (!puzzle[puzzleX].ContainsKey(puzzleY)) {
                    usedPieces.Add(piece);
                    puzzle[puzzleX].Add(puzzleY, piece);
                }
                else {
                    throw new InvalidOperationException("There is already a puzzle piece at position " + puzzleX + ":" + puzzleY);
                }
            }
        }

        private static void RemovePieceFromPuzzle(int puzzleX, int puzzleY, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle, HashSet<PuzzlePiece> usedPieces) {
            if (puzzle.ContainsKey(puzzleX) && puzzle[puzzleX].ContainsKey(puzzleY)) {
                lock (masterLock) {
                    usedPieces.Remove(puzzle[puzzleX][puzzleY]);
                    puzzle[puzzleX].Remove(puzzleY);
                }
            }
        }

        private static void RemovePieceFromPuzzle(PuzzlePiece piece, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle, HashSet<PuzzlePiece> usedPieces) {
            Point pieceCoordinates = DeterminePieceCoordinatesFromPiece(piece, puzzle);
            lock (masterLock) {
                puzzle[pieceCoordinates.X].Remove(pieceCoordinates.Y);
                usedPieces.Remove(piece);
            }
        }

        // helpers

        private static Tuple<int, int, int, int> DetermineMinMaxCoordinates(Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
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
            return new Tuple<int, int, int, int>(minX, maxX, minY, maxY);
        }

        private static Bitmap RenderFullBitmapFromPuzzle(Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            lock (masterLock) {
                Tuple<int, int, int, int> borders = DetermineMinMaxCoordinates(puzzle);

                // create empty image (of correct size)
                Bitmap output = new Bitmap((borders.Item2 - borders.Item1 + 1 + 2 * RENDER_PADDING) * (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH,
                    (borders.Item4 - borders.Item3 + 1 + 2 * RENDER_PADDING) * (PuzzlePiece.DATA_HEIGHT - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH);
                // fill in the pieces we solved
                foreach (KeyValuePair<int, Dictionary<int, PuzzlePiece>> col in puzzle) {
                    int normX = col.Key - borders.Item1 + RENDER_PADDING;
                    foreach (KeyValuePair<int, PuzzlePiece> row in col.Value) {
                        int normY = row.Key - borders.Item3 + RENDER_PADDING;
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
        }

        private void UpdateMasterView() {
            if (PicBoxMaster.InvokeRequired) {
                PicBoxMaster.Invoke(new MethodInvoker(delegate {
                    PicBoxMaster.Image = RenderFullBitmapFromPuzzle(this.masterPuzzle);
                }));
            }
            else {
                PicBoxMaster.Image = RenderFullBitmapFromPuzzle(this.masterPuzzle);
            }
        }

        private static Bitmap RenderPreviewFromPieceCoordinates(Point pieceCoordinates, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            Bitmap bitmap = new Bitmap(3 * (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH,
                3 * (PuzzlePiece.DATA_HEIGHT - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH);
            for (int puzzleY = 0; puzzleY < 3; puzzleY++) {
                for (int puzzleX = 0; puzzleX < 3; puzzleX++) {
                    if (puzzle.ContainsKey(pieceCoordinates.X + puzzleX - 1) && puzzle[pieceCoordinates.X + puzzleX - 1].ContainsKey(pieceCoordinates.Y + puzzleY - 1)) {
                        PuzzlePiece currentPiece = puzzle[pieceCoordinates.X + puzzleX - 1][pieceCoordinates.Y + puzzleY - 1];
                        for (int pieceY = 0; pieceY < PuzzlePiece.DATA_HEIGHT; pieceY++) {
                            for (int pieceX = 0; pieceX < PuzzlePiece.DATA_WIDTH; pieceX++) {
                                Color currentColor = currentPiece.ImageData[pieceX, pieceY];
                                if (!currentColor.IsEmpty) {
                                    bitmap.SetPixel(
                                        puzzleX * (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + pieceX,
                                        puzzleY * (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + pieceY,
                                        currentColor
                                    );
                                }
                            }
                        }
                    }
                }
            }
            return bitmap;
        }

        private static Bitmap RenderPreviewFromPiece(PuzzlePiece centerPiece, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            Point pieceCoordinates = DeterminePieceCoordinatesFromPiece(centerPiece, puzzle);
            return RenderPreviewFromPieceCoordinates(pieceCoordinates, puzzle);
        }

        private static Bitmap RenderPreviewFromBitmapCoordinates(int imageX, int imageY, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            Point pieceCoordinates = DeterminePuzzleCoordinatesFromBitmapCoordinate(imageX, imageY, puzzle);
            return RenderPreviewFromPieceCoordinates(pieceCoordinates, puzzle);
        }

        private static Bitmap GenerateImageFromSinglePiece(PuzzlePiece piece, int zoomFactor = 1) {
            Bitmap bitmap = new Bitmap(PuzzlePiece.DATA_WIDTH, PuzzlePiece.DATA_HEIGHT);
            for (int pieceY = 0; pieceY < PuzzlePiece.DATA_HEIGHT; pieceY++) {
                for (int pieceX = 0; pieceX < PuzzlePiece.DATA_WIDTH; pieceX++) {
                    bitmap.SetPixel(pieceX, pieceY, piece.ImageData[pieceX, pieceY]);
                }
            }
            return new Bitmap(bitmap, PuzzlePiece.DATA_WIDTH * zoomFactor, PuzzlePiece.DATA_HEIGHT * zoomFactor);
        }

        private static Point DeterminePuzzleCoordinatesFromBitmapCoordinate(int imageX, int imageY, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
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
            if ((imageX >= (maxX - minX + 1 + 2 * RENDER_PADDING) * (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH)
                || (imageY >= (maxY - minY + 1 + 2 * RENDER_PADDING) * (PuzzlePiece.DATA_HEIGHT - 2 * PuzzlePiece.BIT_LENGTH) + 2 * PuzzlePiece.BIT_LENGTH)) {
                throw new ArgumentException("Specified coordinates not inside actual image.");
            }

            int normPuzzleX = (imageX - PuzzlePiece.BIT_LENGTH) / (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH) + minX - RENDER_PADDING;
            int normPuzzleY = (imageY - PuzzlePiece.BIT_LENGTH) / (PuzzlePiece.DATA_HEIGHT - 2 * PuzzlePiece.BIT_LENGTH) + minY - RENDER_PADDING;

            return new Point(normPuzzleX, normPuzzleY);
        }

        private static Point DeterminePieceCoordinatesFromPiece(PuzzlePiece piece, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            KeyValuePair<int, Dictionary<int, PuzzlePiece>> targetColumn = puzzle.First(x => x.Value.Any(y => y.Value.Id == piece.Id));
            return new Point(targetColumn.Key, targetColumn.Value.First(x => x.Value.Id == piece.Id).Key);
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

        private static byte GetCounterEdgeFromPiece(PuzzlePiece piece, Direction direction) {
            switch (direction) {
                case Direction.NORTH:
                    return piece.SouthEdge;
                case Direction.SOUTH:
                    return piece.NorthEdge;
                case Direction.EAST:
                    return piece.WestEdge;
                case Direction.WEST:
                    return piece.EastEdge;
                default:
                    throw new ArgumentException("Unknown direction.");
            }
        }

        private static Dictionary<Direction, PuzzlePiece> GetSurroundingPieces(int puzzleX, int puzzleY, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            Dictionary<Direction, PuzzlePiece> surroundingPieces = new Dictionary<Direction, PuzzlePiece>();

            if (puzzle.ContainsKey(puzzleX) && puzzle[puzzleX].ContainsKey(puzzleY - 1)) {
                surroundingPieces.Add(Direction.NORTH, puzzle[puzzleX][puzzleY - 1]);
            }
            if (puzzle.ContainsKey(puzzleX) && puzzle[puzzleX].ContainsKey(puzzleY + 1)) {
                surroundingPieces.Add(Direction.SOUTH, puzzle[puzzleX][puzzleY + 1]);
            }
            if (puzzle.ContainsKey(puzzleX + 1) && puzzle[puzzleX + 1].ContainsKey(puzzleY)) {
                surroundingPieces.Add(Direction.EAST, puzzle[puzzleX + 1][puzzleY]);
            }
            if (puzzle.ContainsKey(puzzleX - 1) && puzzle[puzzleX - 1].ContainsKey(puzzleY)) {
                surroundingPieces.Add(Direction.WEST, puzzle[puzzleX - 1][puzzleY]);
            }

            return surroundingPieces;
        }

        private static Dictionary<Direction, byte> GetSurroundingEdges(int puzzleX, int puzzleY, Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            Dictionary<Direction, byte> surroundingEdges = new Dictionary<Direction, byte>();

            if (puzzle.ContainsKey(puzzleX) && puzzle[puzzleX].ContainsKey(puzzleY - 1)) {
                surroundingEdges.Add(Direction.NORTH, puzzle[puzzleX][puzzleY - 1].SouthEdge);
            }
            if (puzzle.ContainsKey(puzzleX) && puzzle[puzzleX].ContainsKey(puzzleY + 1)) {
                surroundingEdges.Add(Direction.SOUTH, puzzle[puzzleX][puzzleY + 1].NorthEdge);
            }
            if (puzzle.ContainsKey(puzzleX + 1) && puzzle[puzzleX + 1].ContainsKey(puzzleY)) {
                surroundingEdges.Add(Direction.EAST, puzzle[puzzleX + 1][puzzleY].WestEdge);
            }
            if (puzzle.ContainsKey(puzzleX - 1) && puzzle[puzzleX - 1].ContainsKey(puzzleY)) {
                surroundingEdges.Add(Direction.WEST, puzzle[puzzleX - 1][puzzleY].EastEdge);
            }

            return surroundingEdges;
        }

        private static IEnumerable<PuzzlePiece> GetPhysicallyFittingPieces(Dictionary<Direction, byte> surroundingEdges, IEnumerable<PuzzlePiece> availablePieces, IEnumerable<PuzzlePiece> unusablePieces) {
            return availablePieces.Where(x =>
                !unusablePieces.Contains(x) &&
                (!surroundingEdges.ContainsKey(Direction.NORTH) || COUNTER_EDGE_MAP[Direction.NORTH][surroundingEdges[Direction.NORTH]].Contains(x)) &&
                (!surroundingEdges.ContainsKey(Direction.SOUTH) || COUNTER_EDGE_MAP[Direction.SOUTH][surroundingEdges[Direction.SOUTH]].Contains(x)) &&
                (!surroundingEdges.ContainsKey(Direction.EAST) || COUNTER_EDGE_MAP[Direction.EAST][surroundingEdges[Direction.EAST]].Contains(x)) &&
                (!surroundingEdges.ContainsKey(Direction.WEST) || COUNTER_EDGE_MAP[Direction.WEST][surroundingEdges[Direction.WEST]].Contains(x))
            );
        }

        private void PicBoxMaster_Click(object sender, EventArgs e) {
            PictureBox pictureBox = sender as PictureBox;
            MouseEventArgs mouseEventArgs = e as MouseEventArgs;

            this.interruptSolver = true;

            // determine coordinates in picture
            txtBoxLog.AppendText("PicBoxRel " + mouseEventArgs.Location.ToString() + Environment.NewLine);
            Point imageCoordinates = GetImageCoordinatesFromPictureBoxClick(pictureBox, mouseEventArgs.X, mouseEventArgs.Y);
            txtBoxLog.AppendText("ImgRel " + (Point.Empty.Equals(imageCoordinates) ? "N/A" : imageCoordinates.ToString()) + Environment.NewLine);

            // calculate piece in picture
            Point puzzleCoords = DeterminePuzzleCoordinatesFromBitmapCoordinate(imageCoordinates.X, imageCoordinates.Y, this.masterPuzzle);
            txtBoxLog.AppendText("PuzzleC " + puzzleCoords + Environment.NewLine);
            PuzzlePiece clickedPiece = (this.masterPuzzle.ContainsKey(puzzleCoords.X) && this.masterPuzzle[puzzleCoords.X].ContainsKey(puzzleCoords.Y)) ? this.masterPuzzle[puzzleCoords.X][puzzleCoords.Y] : null;
            txtBoxLog.AppendText("Piece #" + clickedPiece + Environment.NewLine);

            if (!Point.Empty.Equals(imageCoordinates)) {
                this.puzzleSelectionCoords = puzzleCoords;
                this.puzzleSelectionEmpty = clickedPiece == null;
            }

            // part of the image was (left) clicked; move selection zoom
            if (!Point.Empty.Equals(imageCoordinates) && (mouseEventArgs.Button == MouseButtons.Left)) {
                PicBoxSelection.Image = RenderPreviewFromBitmapCoordinates(imageCoordinates.X, imageCoordinates.Y, this.masterPuzzle);
            }

            // empty spot in image was (left) clicked; lookup matching parts and add (best ones) to list view
            if (!Point.Empty.Equals(imageCoordinates) && (clickedPiece == null) && (mouseEventArgs.Button == MouseButtons.Left)) {
                ListViewPieceSelection.BeginUpdate();
                this.pieceSelectionImageList.Images.Clear();
                ListViewPieceSelection.Items.Clear();

                Dictionary<Direction, byte> surroundingEdges = GetSurroundingEdges(puzzleCoords.X, puzzleCoords.Y, this.masterPuzzle);
                if (surroundingEdges.Count == 0) {
                    this.LogWithBox(LogLevel.Info, "Clicked empty, random part of picture.");
                    return;
                }
                List<PuzzlePiece> physicallyFittingPieces = GetPhysicallyFittingPieces(surroundingEdges, PIECE_DATA, this.piecesUsedInMaster).ToList();

                List<Tuple<PuzzlePiece, int>> ratedFittingPieces = new List<Tuple<PuzzlePiece, int>>();
                foreach (PuzzlePiece unratedFittingPiece in physicallyFittingPieces) {
                    int distance = PuzzlePiece.GetPieceMultiEdgeDistance(unratedFittingPiece, GetSurroundingPieces(puzzleCoords.X, puzzleCoords.Y, this.masterPuzzle));
                    ratedFittingPieces.Add(new Tuple<PuzzlePiece, int>(unratedFittingPiece, distance));
                }
                ratedFittingPieces.Sort((x, y) => x.Item2.CompareTo(y.Item2));

                foreach (Tuple<PuzzlePiece, int> fittingPieceWithDistance in ratedFittingPieces) {
                    string fittingPieceId = fittingPieceWithDistance.Item1.Id.ToString();
                    this.pieceSelectionImageList.Images.Add(fittingPieceId, GenerateImageFromSinglePiece(fittingPieceWithDistance.Item1, IMAGE_LIST_ZOOM_FACTOR));
                    ListViewItem listViewItem = new ListViewItem("Id:" + fittingPieceId + " Dst:" + fittingPieceWithDistance.Item2, this.pieceSelectionImageList.Images.IndexOfKey(fittingPieceId));
                    listViewItem.Tag = fittingPieceWithDistance.Item1;
                    ListViewPieceSelection.Items.Add(listViewItem);
                }

                ListViewPieceSelection.EndUpdate();
                ListViewPieceSelection.Refresh();
            }

            // right-click remove piece
            if ((clickedPiece != null) && (mouseEventArgs.Button == MouseButtons.Right)) {
                RemovePieceFromPuzzle(clickedPiece, this.masterPuzzle, this.piecesUsedInMaster);
                this.UpdateMasterView();
                this.puzzleSelectionEmpty = true;
            }
        }

        private void ListViewPieceSelection_DoubleClick(object sender, EventArgs e) {
            ListView pieceSelection = sender as ListView;
            if (this.puzzleSelectionEmpty) {
                // just to be sure .. remove the old piece
                RemovePieceFromPuzzle(this.puzzleSelectionCoords.X, this.puzzleSelectionCoords.Y, this.masterPuzzle, this.piecesUsedInMaster);
                AddPieceToPuzzle(this.puzzleSelectionCoords.X, this.puzzleSelectionCoords.Y, pieceSelection.SelectedItems[0].Tag as PuzzlePiece, this.masterPuzzle, this.piecesUsedInMaster);
                this.UpdateMasterView();
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

        // CURRENTLY UNUSED

        private bool TryAddPuzzleToMaster(Dictionary<int, Dictionary<int, PuzzlePiece>> puzzle) {
            // check whether the minimum overlap does exist
            IEnumerable<Tuple<int, int, PuzzlePiece>> masterPieces = this.masterPuzzle.SelectMany(x => x.Value.Select(y => new Tuple<int, int, PuzzlePiece>(x.Key, y.Key, y.Value)));
            bool masterEmpty = masterPieces.Count() == 0;
            IEnumerable<Tuple<int, int, PuzzlePiece>> subPieces = puzzle.SelectMany(x => x.Value.Select(y => new Tuple<int, int, PuzzlePiece>(x.Key, y.Key, y.Value)));
            // just pieces contained in both; offset is still unclear
            IEnumerable<Tuple<int, int, PuzzlePiece>> overlappingPieces = subPieces.Where(x => masterPieces.Any(y => (y.Item3.Id == x.Item3.Id)) || masterEmpty);

            // either the master is empty or a properly overlapping pieces has been found
            int MASTER_ADD_MIN_MATCH_COUNT = 5;
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
                            RemovePieceFromPuzzle(piece.Item1 + properOffset.Item1, piece.Item2 + properOffset.Item2, this.masterPuzzle, this.piecesUsedInMaster);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private void PicBoxMaster_MouseDown(object sender, MouseEventArgs e) {
            PictureBox pictureBox = sender as PictureBox;

            if (e.Button == MouseButtons.Right) {
                Point imageCoords = GetImageCoordinatesFromPictureBoxClick(pictureBox, e.X, e.Y);
                this.deleteMouseDown = DeterminePuzzleCoordinatesFromBitmapCoordinate(imageCoords.X, imageCoords.Y, this.masterPuzzle);
            }
        }

        private void PicBoxMaster_MouseUp(object sender, MouseEventArgs e) {
            PictureBox pictureBox = sender as PictureBox;

            if (e.Button == MouseButtons.Right) {
                Point imageCoords = GetImageCoordinatesFromPictureBoxClick(pictureBox, e.X, e.Y);
                this.deleteMouseUp = DeterminePuzzleCoordinatesFromBitmapCoordinate(imageCoords.X, imageCoords.Y, this.masterPuzzle);

                int minX = Math.Min(this.deleteMouseDown.X, this.deleteMouseUp.X);
                int maxX = Math.Max(this.deleteMouseDown.X, this.deleteMouseUp.X);
                int minY = Math.Min(this.deleteMouseDown.Y, this.deleteMouseUp.Y);
                int maxY = Math.Max(this.deleteMouseDown.Y, this.deleteMouseUp.Y);

                for (int y = minY; y <= maxY; y++) {
                    for (int x = minX; x <= maxX; x++) {
                        RemovePieceFromPuzzle(x, y, this.masterPuzzle, this.piecesUsedInMaster);
                    }
                }

                this.UpdateMasterView();
            }
        }

    }
}
