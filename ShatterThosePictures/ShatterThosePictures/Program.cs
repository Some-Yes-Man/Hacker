using NLog;

namespace ShatterThosePictures {
    internal class Program {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args) {
            Console.WriteLine("Let's shred this picture!!");

            PuzzlePiece[,] pieces;
            using (Image<Rgba32> image = Image.Load<Rgba32>(args[0])) {
                Random prng = new Random((int)new FileInfo(args[0]).Length);

                pieces = new PuzzlePiece[(image.Width - 2 * PuzzlePiece.BIT_LENGTH) / (PuzzlePiece.DATA_WIDTH - 2 * PuzzlePiece.BIT_LENGTH), (image.Height - 2 * PuzzlePiece.BIT_LENGTH) / (PuzzlePiece.DATA_HEIGHT - 2 * PuzzlePiece.BIT_LENGTH)];
                // insert pieces
                int maxValue = (int)Math.Pow(2, PuzzlePiece.BIT_COUNT);
                for (int y = 0; y < pieces.GetLength(1); y++) {
                    for (int x = 0; x < pieces.GetLength(0); x++) {
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

                // match up edges & fill in data
                for (int y = 0; y < pieces.GetLength(1); y++) {
                    for (int x = 0; x < pieces.GetLength(0); x++) {
                        if (x - 1 >= 0) {
                            pieces[x - 1, y].EastEdge = (byte)((maxValue - 1) ^ pieces[x, y].WestEdge);
                        }
                        if (y - 1 >= 0) {
                            pieces[x, y - 1].SouthEdge = (byte)((maxValue - 1) ^ pieces[x, y].NorthEdge);
                        }
                    }
                }

                // fill in data, mask and save to file
                for (int y = 0; y < pieces.GetLength(1); y++) {
                    for (int x = 0; x < pieces.GetLength(0); x++) {
                        pieces[x, y].LoadData(image, x, y);
                        pieces[x, y].SaveToFile(Path.Combine(args[1], "Piece" + x.ToString("D3") + y.ToString("D3") + ".png"));
                    }
                }
                Logger.Info("Done");
            }

        }
    }
}
