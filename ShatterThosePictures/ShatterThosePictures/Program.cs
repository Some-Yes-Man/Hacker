using System.Net.NetworkInformation;

namespace ShatterThosePictures {
    internal class Program {
        static void Main(string[] args) {
            Console.WriteLine("Let's shred this picture!!");

            PuzzlePiece[,] pieces;
            using (Image<Rgb24> image = Image.Load<Rgb24>(args[0])) {
                Random prng = new Random((int)new FileInfo(args[0]).Length);

                pieces = new PuzzlePiece[(image.Width - 1) / PuzzlePiece.DATA_WIDTH, (image.Height - 1) / PuzzlePiece.DATA_HEIGHT];
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

                // match up edges
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

                image.ProcessPixelRows(accessor => {
                });
            }
        }
    }
}