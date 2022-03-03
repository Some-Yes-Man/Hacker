using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShatteredAndShuffledBruteForce {
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

        private static int ID_GENERATOR = 0;

        public int Id { get; private set; }
        public byte NorthEdge { get; set; }
        public byte SouthEdge { get; set; }
        public byte EastEdge { get; set; }
        public byte WestEdge { get; set; }
        public HashSet<Direction> PuzzleEdges { get; set; }
        public Color[,] ImageData { get; set; }
        public string Filename { get; set; }

        private PuzzlePiece() { }

        public PuzzlePiece(string filename) {
            this.Id = ++ID_GENERATOR;
            this.Filename = filename;
            this.PuzzleEdges = new HashSet<Direction>();
            this.ImageData = new Color[DATA_WIDTH, DATA_HEIGHT];

            // get initial pixel & data area
            LOGGER.Trace("Reading '{filename}'.", filename);
            Bitmap bitmap = new Bitmap(filename);
            Color backgroundColor = bitmap.GetPixel(0, 0);
            byte startX = 0;
            byte startY = 0;
            while (!BitmapColIsCore(bitmap, startX, backgroundColor)) {
                startX++;
            }
            while (!BitmapRowIsCore(bitmap, startY, backgroundColor)) {
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
        }

        public override int GetHashCode() {
            return this.Id;
        }

        public override bool Equals(object obj) {
            PuzzlePiece other = obj as PuzzlePiece;
            if (other == null) {
                return false;
            }
            return this.Id.Equals(other.Id);
        }

        public override string ToString() {
            return this.Id.ToString();
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

    }
}
