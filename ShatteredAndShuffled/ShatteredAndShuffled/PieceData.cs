using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace ShatteredAndShuffled {
    public class PieceData {

        public const byte EGDE_LENGTH = 5;
        public const byte DATA_WIDTH = 24;
        public const byte DATA_HEIGHT = 24;

        public enum DIRECTION {
            NORTH, SOUTH, EAST, WEST
        }

        public class ColorTupleRGB {
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }
            public int Sum { get { return this.R + this.G + this.B; } }

            public ColorTupleRGB() { }

            public ColorTupleRGB(byte r, byte g, byte b) {
                this.R = r;
                this.G = g;
                this.B = b;
            }

            public static int GetQuadraticDifference(ColorTupleRGB a, ColorTupleRGB b) {
                return (a.R - b.R) * (a.R - b.R) + (a.G - b.G) * (a.G - b.G) + (a.B - b.B) * (a.B - b.B);
            }
        }

        public string Filename { get; set; }
        public bool[] NorthPattern { get; set; }
        public bool[] SouthPattern { get; set; }
        public bool[] EastPattern { get; set; }
        public bool[] WestPattern { get; set; }
        public bool IsBorderNorth { get; set; }
        public bool IsBorderSouth { get; set; }
        public bool IsBorderEast { get; set; }
        public bool IsBorderWest { get; set; }
        public ColorTupleRGB[] ImageData { get; set; }

        public ColorTupleRGB GetImagePixel(int x, int y) {
            return this.ImageData[y * DATA_WIDTH + x];
        }

        [XmlIgnore]
        public bool IsCorner {
            get {
                return (this.IsBorderNorth && this.IsBorderWest) || (this.IsBorderNorth && this.IsBorderEast) || (this.IsBorderSouth && this.IsBorderWest) || (this.IsBorderSouth && this.IsBorderEast);
            }
        }
        [XmlIgnore]
        public bool AlreadyUsed { get; set; }

        private PieceData() { }

        public static bool CompareColorsRGB(Color a, Color b) {
            return ((a.R == b.R) && (a.G == b.G) && (a.B == b.B));
        }

        private static bool EdgePatternMatches(bool[] a, bool[] b) {
            for (int i = 0; i < EGDE_LENGTH; i++) {
                if (!(a[i] ^ b[i])) {
                    return false;
                }
            }
            return true;
        }

        public static bool EdgePatternMatches(PieceData a, PieceData b, DIRECTION d) {
            switch (d) {
                case DIRECTION.NORTH:
                    return EdgePatternMatches(a.NorthPattern, b.SouthPattern);
                case DIRECTION.SOUTH:
                    return EdgePatternMatches(a.SouthPattern, b.NorthPattern);
                case DIRECTION.EAST:
                    return EdgePatternMatches(a.EastPattern, b.WestPattern);
                case DIRECTION.WEST:
                    return EdgePatternMatches(a.WestPattern, b.EastPattern);
                default:
                    return false;
            }
        }

        public static List<Tuple<PieceData, long>> GuestimateMatchingPiece(PieceData given, List<PieceData> possible, DIRECTION dir) {
            // no elements were passed
            if ((given == null) || (possible.Count == 0)) {
                Console.WriteLine("Problem while finding pieces!");
                return new List<Tuple<PieceData, long>>();
            }
            // exactly one element left
            if (possible.Count == 1) {
                return new List<Tuple<PieceData, long>>(new Tuple<PieceData, long>[] { new Tuple<PieceData, long>(possible[0], 0) });
            }
            // more than one element left
            List<Tuple<PieceData, long>> differences = new List<Tuple<PieceData, long>>();
            foreach (PieceData piece in possible) {
                long diff = 0;
                switch (dir) {
                    case DIRECTION.NORTH:
                        for (int x = 2; x < DATA_WIDTH - 2; x++) {
                            for (int givenY = 0; givenY <= 4; givenY++) {
                                if (given.GetImagePixel(x, givenY).Sum != 765) {
                                    for (int pieceY = 0; pieceY <= 4; pieceY++) {
                                        if (piece.GetImagePixel(x, DATA_HEIGHT - pieceY - 1).Sum != 765) {
                                            diff += ColorTupleRGB.GetQuadraticDifference(given.GetImagePixel(x, givenY), piece.GetImagePixel(x, DATA_HEIGHT - pieceY - 1));
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                    case DIRECTION.SOUTH:
                        for (int x = 2; x < DATA_WIDTH - 2; x++) {
                            for (int givenY = 0; givenY <= 4; givenY++) {
                                if (given.GetImagePixel(x, DATA_HEIGHT - givenY - 1).Sum != 765) {
                                    for (int pieceY = 0; pieceY <= 4; pieceY++) {
                                        if (piece.GetImagePixel(x, pieceY).Sum != 765) {
                                            diff += ColorTupleRGB.GetQuadraticDifference(given.GetImagePixel(x, DATA_HEIGHT - givenY - 1), piece.GetImagePixel(x, pieceY));
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                    case DIRECTION.EAST:
                        for (int y = 2; y < DATA_HEIGHT - 2; y++) {
                            for (int givenX = 0; givenX <= 4; givenX++) {
                                if (given.GetImagePixel(DATA_WIDTH - givenX - 1, y).Sum != 765) {
                                    for (int pieceX = 0; pieceX <= 4; pieceX++) {
                                        if (piece.GetImagePixel(pieceX, y).Sum != 765) {
                                            diff += ColorTupleRGB.GetQuadraticDifference(given.GetImagePixel(DATA_HEIGHT - givenX - 1, y), piece.GetImagePixel(pieceX, y));
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                    case DIRECTION.WEST:
                        for (int y = 2; y < DATA_HEIGHT - 2; y++) {
                            for (int givenX = 0; givenX <= 4; givenX++) {
                                if (given.GetImagePixel(givenX, y).Sum != 765) {
                                    for (int pieceX = 0; pieceX <= 4; pieceX++) {
                                        if (piece.GetImagePixel(DATA_WIDTH - pieceX - 1, y).Sum != 765) {
                                            diff += ColorTupleRGB.GetQuadraticDifference(given.GetImagePixel(givenX, y), piece.GetImagePixel(DATA_HEIGHT - pieceX - 1, y));
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                }
                differences.Add(new Tuple<PieceData, long>(piece, diff));
            }
            Console.WriteLine("Guestimate from " + possible.Count + " items: " + string.Join(" , ", differences.Select(x => x.Item2 + ":" + x.Item1.Filename)));

            return differences;
        }

        public PieceData(string filename, Bitmap bitmap) {
            this.Filename = filename;
            this.NorthPattern = new bool[EGDE_LENGTH];
            for (int i = 0; i < EGDE_LENGTH; i++) {
                if (CompareColorsRGB(bitmap.GetPixel(7 + i * 3, 3), Color.White) ^ CompareColorsRGB(bitmap.GetPixel(7 + i * 3, 4), Color.White)) {
                    this.IsBorderNorth = true;
                    break;
                }
                this.NorthPattern[i] = CompareColorsRGB(bitmap.GetPixel(7 + i * 3, 3), Color.White) && CompareColorsRGB(bitmap.GetPixel(7 + i * 3, 4), Color.White);
            }
            this.SouthPattern = new bool[EGDE_LENGTH];
            for (int i = 0; i < EGDE_LENGTH; i++) {
                if (CompareColorsRGB(bitmap.GetPixel(7 + i * 3, 23), Color.White) ^ CompareColorsRGB(bitmap.GetPixel(7 + i * 3, 24), Color.White)) {
                    this.IsBorderSouth = true;
                    break;
                }
                this.SouthPattern[i] = CompareColorsRGB(bitmap.GetPixel(7 + i * 3, 23), Color.White) && CompareColorsRGB(bitmap.GetPixel(7 + i * 3, 24), Color.White);
            }
            this.EastPattern = new bool[EGDE_LENGTH];
            for (int i = 0; i < EGDE_LENGTH; i++) {
                if (CompareColorsRGB(bitmap.GetPixel(23, 7 + i * 3), Color.White) ^ CompareColorsRGB(bitmap.GetPixel(24, 7 + i * 3), Color.White)) {
                    this.IsBorderEast = true;
                    break;
                }
                this.EastPattern[i] = CompareColorsRGB(bitmap.GetPixel(23, 7 + i * 3), Color.White) && CompareColorsRGB(bitmap.GetPixel(24, 7 + i * 3), Color.White);
            }
            this.WestPattern = new bool[EGDE_LENGTH];
            for (int i = 0; i < EGDE_LENGTH; i++) {
                if (CompareColorsRGB(bitmap.GetPixel(3, 7 + i * 3), Color.White) ^ CompareColorsRGB(bitmap.GetPixel(4, 7 + i * 3), Color.White)) {
                    this.IsBorderWest = true;
                    break;
                }
                this.WestPattern[i] = CompareColorsRGB(bitmap.GetPixel(3, 7 + i * 3), Color.White) && CompareColorsRGB(bitmap.GetPixel(4, 7 + i * 3), Color.White);
            }

            this.ImageData = new ColorTupleRGB[DATA_WIDTH * DATA_HEIGHT];
            for (int y = 0; y < DATA_HEIGHT; y++) {
                for (int x = 0; x < DATA_WIDTH; x++) {
                    Color foo = bitmap.GetPixel(x + 2, y + 2);
                    this.ImageData[y * DATA_HEIGHT + x] = new ColorTupleRGB(foo.R, foo.G, foo.B);
                }
            }
        }
    }
}
