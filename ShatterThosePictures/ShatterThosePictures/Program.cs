using NLog;

namespace ShatterThosePictures {
    internal class Program {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Direction[] DIRECTIONS = new Direction[4] { Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST };

        static void Main(string[] args) {
            Logger.Info("Let's shred this picture!!");

            PuzzlePiece[,] pieces;
            using (Image<Rgba32> image = Image.Load<Rgba32>(args[0])) {
                Random prng = new Random((int)new FileInfo(args[0]).Length);

                int piecesWidth = (image.Width - 2 * PuzzlePiece.BIT_LENGTH) / (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH);
                int piecesHeight = (image.Height - 2 * PuzzlePiece.BIT_LENGTH) / (PuzzlePiece.DATA_HEIGHT - 2 * PuzzlePiece.BIT_LENGTH);
                pieces = new PuzzlePiece[piecesWidth, piecesHeight];
                // insert pieces
                int maxValue = (int)Math.Pow(2, PuzzlePiece.BIT_COUNT);
                for (int y = 0; y < piecesHeight; y++) {
                    for (int x = 0; x < piecesWidth; x++) {
                        pieces[x, y] = new PuzzlePiece();
                        pieces[x, y].PuzzleEdges.Add(Direction.NORTH);
                        pieces[x, y].NorthEdge = (byte)prng.Next(maxValue);
                        pieces[x, y].PuzzleEdges.Add(Direction.SOUTH);
                        pieces[x, y].SouthEdge = (byte)prng.Next(maxValue);
                        pieces[x, y].PuzzleEdges.Add(Direction.EAST);
                        pieces[x, y].EastEdge = (byte)prng.Next(maxValue);
                        pieces[x, y].PuzzleEdges.Add(Direction.WEST);
                        pieces[x, y].WestEdge = (byte)prng.Next(maxValue);
                    }
                }
                Logger.Info("Pieces initialized.");

                // match up edges & fill in data
                for (int y = 0; y < piecesHeight; y++) {
                    for (int x = 0; x < piecesWidth; x++) {
                        if (x - 1 >= 0) {
                            pieces[x - 1, y].EastEdge = (byte)((maxValue - 1) ^ pieces[x, y].WestEdge);
                        }
                        if (y - 1 >= 0) {
                            pieces[x, y - 1].SouthEdge = (byte)((maxValue - 1) ^ pieces[x, y].NorthEdge);
                        }
                    }
                }
                Logger.Info("Edges lined up.");

                // fill in data, mask and save to file
                for (int y = 0; y < piecesHeight; y++) {
                    for (int x = 0; x < piecesWidth; x++) {
                        pieces[x, y].LoadData(image, x, y);
                        //pieces[x, y].SaveToFile(Path.Combine(args[1], "Piece" + x.ToString("D3") + y.ToString("D3") + ".png"));
                    }
                }
                Logger.Info("Copied and masked image data.");

                // determine all matches for every piece
                int pieceCount = piecesWidth * piecesHeight;
                List<Tuple<int, int, PuzzlePiece>>[,,] pieceMatches = new List<Tuple<int, int, PuzzlePiece>>[piecesWidth, piecesHeight, 4];

                int countMatches = 0;
                for (int d = 0; d < 4; d++) {
                    // iterating index-based to avoid duplication in matching
                    for (int indexOne = 0; indexOne < pieceCount; indexOne++) {
                        int oneX = indexOne % piecesWidth;
                        int oneY = indexOne / piecesWidth;
                        PuzzlePiece pieceOne = pieces[oneX, oneY];
                        pieceMatches[oneX, oneY, d] = new();
                        for (int indexTwo = indexOne + 1; indexTwo < pieceCount; indexTwo++) {
                            int twoX = indexTwo % piecesWidth;
                            int twoY = indexTwo / piecesWidth;
                            PuzzlePiece pieceTwo = pieces[twoX, twoY];
                            if (PuzzlePiece.EdgeMatches(pieceOne, pieceTwo, DIRECTIONS[d])) {
                                countMatches++;
                                pieceMatches[oneX, oneY, d].Add(new Tuple<int, int, PuzzlePiece>(twoX, twoY, pieceTwo));
                            }
                        }
                    }
                    Logger.Info("Matching {} edges.", DIRECTIONS[d]);
                }
                Logger.Info("Found {} matches.", countMatches);

                Logger.Info("Saving training data.");
                for (int y = 0; y < piecesHeight; y++) {
                    for (int x = 0; x < piecesWidth; x++) {
                        for (int d = 0; d < 1; d++) {
                            int matchCount = pieceMatches[x, y, d].Count;
                            for (int m = 0; m < matchCount; m++) {
                                Tuple<int, int, PuzzlePiece> match = pieceMatches[x, y, d][m];
                                PuzzlePiece.SaveTrainingData(pieces[x, y], match.Item3, DIRECTIONS[d], Path.Combine(args[2],
                                    string.Format("{0:D3}{1:D3}-{2:D3}{3:D3}-{4}.png", x, y, match.Item1, match.Item2, d)));
                            }
                        }
                    }
                }

                Logger.Info("Done!");
            }

        }
    }
}
