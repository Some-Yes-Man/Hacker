using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShreddedAndScrambled {
    public class PuzzlePiece {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        // puzzle #2
        //public const byte DATA_WIDTH = 18;
        //public const byte DATA_HEIGHT = 18;
        //public const byte BIT_COUNT = 3;
        // puzzle #1
        public const byte DATA_WIDTH = 24;
        public const byte DATA_HEIGHT = 24;
        public const byte BIT_COUNT = 5;

        public const byte BIT_THICCNESS = 2;
        public const byte BIT_OFFSET = 5;
        public const byte BIT_SPACING = 1;
        public const byte BIT_LENGTH = 2;

        public static readonly byte EDGE_MATCH_BYTE = (byte)(Math.Pow(2, BIT_COUNT) - 1);

        public byte NorthEdge { get; set; }
        public byte SouthEdge { get; set; }
        public byte EastEdge { get; set; }
        public byte WestEdge { get; set; }
        public HashSet<Direction> PuzzleEdges { get; set; }
        public Color[] NorthKeys { get; set; }
        public Color[] SouthKeys { get; set; }
        public Color[] EastKeys { get; set; }
        public Color[] WestKeys { get; set; }
        public Color[] NorthKeysHsl { get; set; }
        public Color[] SouthKeysHsl { get; set; }
        public Color[] EastKeysHsl { get; set; }
        public Color[] WestKeysHsl { get; set; }
        public Color[,] ImageData { get; set; }
        public Color AverageColor { get; set; }
        public Color DeviationColor { get; set; }
        public string Filename { get; set; }

        private PuzzlePiece() { }

        public PuzzlePiece(string filename) {
            this.Filename = filename;
            this.PuzzleEdges = new HashSet<Direction>();
            this.ImageData = new Color[DATA_WIDTH, DATA_HEIGHT];

            // get initial pixel & data area
            LOGGER.Trace("Reading '{filename}'.", filename);
            Bitmap bitmap = new Bitmap(filename);
            Color backgroundColor = bitmap.GetPixel(0, 0);
            byte startX = 0;
            byte startY = 0;
            while (!PuzzlePiece.BitmapColIsCore(bitmap, startX, backgroundColor)) {
                startX++;
            }
            while (!PuzzlePiece.BitmapRowIsCore(bitmap, startY, backgroundColor)) {
                startY++;
            }
            // remove bit length from starting positions to catch outer bits
            startX -= BIT_LENGTH;
            startY -= BIT_LENGTH;
            LOGGER.Trace("Found image data at {x}:{y}.", startX, startY);

            // get image data
            for (byte y = startY; y < (startY + DATA_HEIGHT); y++) {
                for (byte x = startX; x < (startX + DATA_WIDTH); x++) {
                    Color color = bitmap.GetPixel(x, y);
                    this.ImageData[x - startX, y - startY] = color.Equals(backgroundColor) ? Color.Empty : color;
                }
            }

            // EDGES
            byte northInnerEdge = 0;
            for (int n = 0; n < BIT_COUNT; n++) {
                Color outerBitPixelColor = this.ImageData[BIT_OFFSET + n * (BIT_THICCNESS + BIT_SPACING), 0];
                Color innerBitPixelColor = this.ImageData[BIT_OFFSET + n * (BIT_THICCNESS + BIT_SPACING), BIT_LENGTH];
                if (!Color.Empty.Equals(outerBitPixelColor)) {
                    this.NorthEdge += (byte)Math.Pow(2, n);
                }
                if (!Color.Empty.Equals(innerBitPixelColor)) {
                    northInnerEdge += (byte)Math.Pow(2, n);
                }
            }
            if (this.NorthEdge != northInnerEdge) {
                this.PuzzleEdges.Add(Direction.NORTH);
            }

            byte southInnerEdge = 0;
            for (int s = 0; s < BIT_COUNT; s++) {
                Color outerBitPixelColor = this.ImageData[BIT_OFFSET + s * (BIT_THICCNESS + BIT_SPACING), DATA_HEIGHT - 1];
                Color innerBitPixelColor = this.ImageData[BIT_OFFSET + s * (BIT_THICCNESS + BIT_SPACING), DATA_HEIGHT - 1 - BIT_LENGTH];
                if (!Color.Empty.Equals(outerBitPixelColor)) {
                    this.SouthEdge += (byte)Math.Pow(2, s);
                }
                if (!Color.Empty.Equals(innerBitPixelColor)) {
                    southInnerEdge += (byte)Math.Pow(2, s);
                }
            }
            if (this.SouthEdge != southInnerEdge) {
                this.PuzzleEdges.Add(Direction.SOUTH);
            }

            byte eastInnerEdge = 0;
            for (int e = 0; e < BIT_COUNT; e++) {
                Color outerBitPixelColor = this.ImageData[DATA_WIDTH - 1, BIT_OFFSET + e * (BIT_THICCNESS + BIT_SPACING)];
                Color innerBitPixelColor = this.ImageData[DATA_WIDTH - 1 - BIT_LENGTH, BIT_OFFSET + e * (BIT_THICCNESS + BIT_SPACING)];
                if (!Color.Empty.Equals(outerBitPixelColor)) {
                    this.EastEdge += (byte)Math.Pow(2, e);
                }
                if (!Color.Empty.Equals(innerBitPixelColor)) {
                    eastInnerEdge += (byte)Math.Pow(2, e);
                }
            }
            if (this.EastEdge != eastInnerEdge) {
                this.PuzzleEdges.Add(Direction.EAST);
            }

            byte westInnerEdge = 0;
            for (int w = 0; w < BIT_COUNT; w++) {
                Color outerBitPixelColor = this.ImageData[0, BIT_OFFSET + w * (BIT_THICCNESS + BIT_SPACING)];
                Color innerBitPixelColor = this.ImageData[BIT_LENGTH, BIT_OFFSET + w * (BIT_THICCNESS + BIT_SPACING)];
                if (!Color.Empty.Equals(outerBitPixelColor)) {
                    this.WestEdge += (byte)Math.Pow(2, w);
                }
                if (!Color.Empty.Equals(innerBitPixelColor)) {
                    westInnerEdge += (byte)Math.Pow(2, w);
                }
            }
            if (this.WestEdge != westInnerEdge) {
                this.PuzzleEdges.Add(Direction.WEST);
            }

            // KEYS
            this.NorthKeys = new Color[DATA_WIDTH - 2 * BIT_LENGTH];
            this.NorthKeysHsl = new Color[DATA_WIDTH - 2 * BIT_LENGTH];
            for (int n = 0; n < this.NorthKeys.Length; n++) {
                this.NorthKeys[n] = this.FindFirstColorInColumn(n, backgroundColor);
                this.NorthKeysHsl[n] = Color.FromArgb((int)Math.Floor(this.NorthKeys[n].GetHue() / 360 * 255), (int)Math.Floor(this.NorthKeys[n].GetSaturation() * 255), (int)Math.Floor(this.NorthKeys[n].GetBrightness() * 255));
            }
            this.SouthKeys = new Color[DATA_WIDTH - 2 * BIT_LENGTH];
            this.SouthKeysHsl = new Color[DATA_WIDTH - 2 * BIT_LENGTH];
            for (int s = 0; s < this.SouthKeys.Length; s++) {
                this.SouthKeys[s] = this.FindLastColorInColumn(s, backgroundColor);
                this.SouthKeysHsl[s] = Color.FromArgb((int)Math.Floor(this.SouthKeys[s].GetHue() / 360 * 255), (int)Math.Floor(this.SouthKeys[s].GetSaturation() * 255), (int)Math.Floor(this.SouthKeys[s].GetBrightness() * 255));
            }
            this.EastKeys = new Color[DATA_HEIGHT - 2 * BIT_LENGTH];
            this.EastKeysHsl = new Color[DATA_HEIGHT - 2 * BIT_LENGTH];
            for (int e = 0; e < this.EastKeys.Length; e++) {
                this.EastKeys[e] = this.FindLastColorInRow(e, backgroundColor);
                this.EastKeysHsl[e] = Color.FromArgb((int)Math.Floor(this.EastKeys[e].GetHue() / 360 * 255), (int)Math.Floor(this.EastKeys[e].GetSaturation() * 255), (int)Math.Floor(this.EastKeys[e].GetBrightness() * 255));
            }
            this.WestKeys = new Color[DATA_HEIGHT - 2 * BIT_LENGTH];
            this.WestKeysHsl = new Color[DATA_HEIGHT - 2 * BIT_LENGTH];
            for (int w = 0; w < this.WestKeys.Length; w++) {
                this.WestKeys[w] = this.FindFirstColorInRow(w, backgroundColor);
                this.WestKeysHsl[w] = Color.FromArgb((int)Math.Floor(this.WestKeys[w].GetHue() / 360 * 255), (int)Math.Floor(this.WestKeys[w].GetSaturation() * 255), (int)Math.Floor(this.WestKeys[w].GetBrightness() * 255));
            }

            // CORE IMAGE AVERAGE & VARIANCE
            double avgSumR = 0;
            double avgSumG = 0;
            double avgsumB = 0;
            for (int y = BIT_LENGTH * 2; y < DATA_HEIGHT - 2 * BIT_LENGTH; y++) {
                for (int x = BIT_LENGTH * 2; x < DATA_WIDTH - 2 * BIT_LENGTH; x++) {
                    avgSumR += this.ImageData[x, y].R;
                    avgSumG += this.ImageData[x, y].G;
                    avgsumB += this.ImageData[x, y].B;
                }
            }
            int coreArea = (DATA_HEIGHT - 4 * BIT_LENGTH) * (DATA_WIDTH - 4 * BIT_LENGTH);
            this.AverageColor = Color.FromArgb((int)Math.Floor(avgSumR / coreArea), (int)Math.Floor(avgSumG / coreArea), (int)Math.Floor(avgsumB / coreArea));

            double varSumR = 0;
            double varSumG = 0;
            double varSumB = 0;
            for (int y = BIT_LENGTH * 2; y < DATA_HEIGHT - 2 * BIT_LENGTH; y++) {
                for (int x = BIT_LENGTH * 2; x < DATA_WIDTH - 2 * BIT_LENGTH; x++) {
                    varSumR += Math.Pow(this.ImageData[x, y].R - this.AverageColor.R, 2);
                    varSumG += Math.Pow(this.ImageData[x, y].G - this.AverageColor.G, 2);
                    varSumB += Math.Pow(this.ImageData[x, y].B - this.AverageColor.B, 2);
                }
            }
            this.DeviationColor = Color.FromArgb((int)Math.Floor(Math.Sqrt(varSumR / coreArea)), (int)Math.Floor(Math.Sqrt(varSumG / coreArea)), (int)Math.Floor(Math.Sqrt(varSumB / coreArea)));

            // TODO: include corner colors?
        }

        private static bool BitmapRowIsCore(Bitmap bitmap, byte y, Color backgroundColor) {
            byte consecutiveDataPixels = 0;
            for (byte x = 0; x < bitmap.Width; x++) {
                if (!bitmap.GetPixel(x, y).Equals(backgroundColor)) {
                    consecutiveDataPixels++;
                }
                else {
                    consecutiveDataPixels = 0;
                }
                if (consecutiveDataPixels == 3) {
                    return true;
                }
            }
            return false;
        }

        private static bool BitmapColIsCore(Bitmap bitmap, byte x, Color backgroundColor) {
            byte consecutiveDataPixels = 0;
            for (byte y = 0; y < bitmap.Height; y++) {
                if (!bitmap.GetPixel(x, y).Equals(backgroundColor)) {
                    consecutiveDataPixels++;
                }
                else {
                    consecutiveDataPixels = 0;
                }
                if (consecutiveDataPixels == 3) {
                    return true;
                }
            }
            return false;
        }

        private Color FindFirstColorInColumn(int x, Color backgroundColor) {
            for (int i = BIT_LENGTH; i < DATA_HEIGHT - BIT_LENGTH; i++) {
                if (!this.ImageData[x + BIT_LENGTH, i].IsEmpty) {
                    return this.ImageData[x + BIT_LENGTH, i];
                }
            }
            throw new IndexOutOfRangeException("Could not find something other than the background color.");
        }

        private Color FindLastColorInColumn(int x, Color backgroundColor) {
            for (int i = DATA_HEIGHT - BIT_LENGTH - 1; i >= BIT_LENGTH; i--) {
                if (!this.ImageData[x + BIT_LENGTH, i].IsEmpty) {
                    return this.ImageData[x + BIT_LENGTH, i];
                }
            }
            throw new IndexOutOfRangeException("Could not find something other than the background color.");
        }

        private Color FindFirstColorInRow(int y, Color backgroundColor) {
            for (int i = BIT_LENGTH; i < DATA_WIDTH - BIT_LENGTH; i++) {
                if (!this.ImageData[i, y + BIT_LENGTH].IsEmpty) {
                    return this.ImageData[i, y + BIT_LENGTH];
                }
            }
            throw new IndexOutOfRangeException("Could not find something other than the background color.");
        }

        private Color FindLastColorInRow(int y, Color backgroundColor) {
            for (int i = DATA_WIDTH - BIT_LENGTH - 1; i >= BIT_LENGTH; i--) {
                if (!this.ImageData[i, y + BIT_LENGTH].IsEmpty) {
                    return this.ImageData[i, y + BIT_LENGTH];
                }
            }
            throw new IndexOutOfRangeException("Could not find something other than the background color.");
        }

        private static bool EdgeMatches(byte edgeA, byte edgeB) {
            return (edgeA ^ edgeB) == EDGE_MATCH_BYTE;
        }

        public static byte InvertEdge(byte edge) {
            return (byte)(edge ^ EDGE_MATCH_BYTE);
        }

        public static bool EdgeMatches(PuzzlePiece pieceA, PuzzlePiece pieceB, Direction edgeOnA) {
            switch (edgeOnA) {
                case Direction.NORTH:
                    return EdgeMatches(pieceA.NorthEdge, pieceB.SouthEdge);
                case Direction.SOUTH:
                    return EdgeMatches(pieceA.SouthEdge, pieceB.NorthEdge);
                case Direction.EAST:
                    return EdgeMatches(pieceA.EastEdge, pieceB.WestEdge);
                case Direction.WEST:
                    return EdgeMatches(pieceA.WestEdge, pieceB.EastEdge);
                default:
                    return false;
            }
        }

        public bool EdgeMatches(PuzzlePiece otherPiece, Direction edgeOnThis) {
            return EdgeMatches(this, otherPiece, edgeOnThis);
        }

        public static int ColorDistance(Color colorA, Color colorB) {
            int r = colorA.R - colorB.R;
            int g = colorA.G - colorB.G;
            int b = colorA.B - colorB.B;
            return r * r + g * g + b * b;
        }

        public static int ColorDistanceWiki(Color colorA, Color colorB) {
            int meanR = (colorA.R + colorB.R) / 2;
            double diffR = (colorA.R - colorB.R);
            double diffG = (colorA.G - colorB.G);
            double diffB = (colorA.B - colorB.B);
            return (int)Math.Sqrt((diffR * diffR * (2 + meanR / 256)) + (diffG * diffG * 4) + (diffB * diffB * (2 + (255 - meanR) / 256)));
        }

        public static int GetRgbDistance(Color[] keysA, Color[] keysB) {
            int distance = 0;
            for (int i = 0; i < BIT_COUNT + 1; i++) {
                distance += PuzzlePiece.ColorDistanceWiki(keysA[i], keysB[i]);
            }
            return distance;
        }

        public static int GetHslDistance(Color[] keysA, Color[] keysB) {
            int distance = 0;
            for (int i = 0; i < BIT_COUNT + 1; i++) {
                distance += PuzzlePiece.ColorDistance(keysA[i], keysB[i]);
            }
            return distance;
        }

    }
}
