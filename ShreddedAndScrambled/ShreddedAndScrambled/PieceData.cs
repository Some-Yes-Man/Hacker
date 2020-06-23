using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Serialization;

namespace ShreddedAndScrambled {
    public class PieceData {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public const byte BIT_COUNT = 3;
        public const byte BIT_THICCNESS = 2;
        public const byte BIT_OFFSET = 5;
        public const byte BIT_SPACING = 1;
        public const byte BIT_LENGTH = 2;
        public const byte DATA_WIDTH = 18;
        public const byte DATA_HEIGHT = 18;

        public enum Direction {
            NORTH, SOUTH, EAST, WEST
        }

        public byte NorthEdge { get; set; }
        public byte SouthEdge { get; set; }
        public byte EastEdge { get; set; }
        public byte WestEdge { get; set; }
        public Color[,] ImageData { get; set; }
        public bool AlreadyUsed { get; set; }
        public string Filename { get; set; }

        private PieceData() { }

        public PieceData(string filename) {
            this.ImageData = new Color[DATA_WIDTH, DATA_HEIGHT];

            // get initial pixel & data area
            LOGGER.Trace("Reading '{filename}'.", filename);
            Bitmap bitmap = new Bitmap(filename);
            Color backgroundColor = bitmap.GetPixel(0, 0);
            byte startX = 0;
            byte startY = 0;
            while (!PieceData.BitmapColIsCore(bitmap, startX, backgroundColor)) {
                startX++;
            }
            while (!PieceData.BitmapRowIsCore(bitmap, startY, backgroundColor)) {
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
            // get north edge
            for (int n = 0; n < BIT_COUNT; n++) {
                Color bitPixelColor = this.ImageData[BIT_OFFSET + n * (BIT_THICCNESS + BIT_SPACING), 0];
                if (!Color.Empty.Equals(bitPixelColor)) {
                    this.NorthEdge += (byte)Math.Pow(2, n);
                }
            }
            // get south edge
            for (int s = 0; s < BIT_COUNT; s++) {
                Color bitPixelColor = this.ImageData[BIT_OFFSET + s * (BIT_THICCNESS + BIT_SPACING), DATA_HEIGHT - 1];
                if (!Color.Empty.Equals(bitPixelColor)) {
                    this.SouthEdge += (byte)Math.Pow(2, s);
                }
            }
            // get east edge
            for (int e = 0; e < BIT_COUNT; e++) {
                Color bitPixelColor = this.ImageData[0, BIT_OFFSET + e * (BIT_THICCNESS + BIT_SPACING)];
                if (!Color.Empty.Equals(bitPixelColor)) {
                    this.EastEdge += (byte)Math.Pow(2, e);
                }
            }
            // get west edge
            for (int w = 0; w < BIT_COUNT; w++) {
                Color bitPixelColor = this.ImageData[DATA_WIDTH - 1, BIT_OFFSET + w * (BIT_THICCNESS + BIT_SPACING)];
                if (!Color.Empty.Equals(bitPixelColor)) {
                    this.WestEdge += (byte)Math.Pow(2, w);
                }
            }
        }

        private static bool BitmapRowIsCore(Bitmap bitmap, byte y, Color backgroundColor) {
            byte dataPixelCount = 0;
            for (byte x = 0; x < bitmap.Width; x++) {
                if (!bitmap.GetPixel(x, y).Equals(backgroundColor)) {
                    dataPixelCount++;
                }
            }
            return dataPixelCount > BIT_COUNT * BIT_THICCNESS;
        }

        private static bool BitmapColIsCore(Bitmap bitmap, byte x, Color backgroundColor) {
            byte dataPixelCount = 0;
            for (byte y = 0; y < bitmap.Width; y++) {
                if (!bitmap.GetPixel(x, y).Equals(backgroundColor)) {
                    dataPixelCount++;
                }
            }
            return dataPixelCount > BIT_COUNT * BIT_THICCNESS;
        }

        public static bool EdgePatternMatches(byte edgeA, byte edgeB) {
            return (edgeA ^ edgeB) == 7;
        }

        public static bool EdgePatternMatches(PieceData pieceA, PieceData pieceB, Direction edgeOnA) {
            switch (edgeOnA) {
                case Direction.NORTH:
                    return EdgePatternMatches(pieceA.NorthEdge, pieceB.SouthEdge);
                case Direction.SOUTH:
                    return EdgePatternMatches(pieceA.SouthEdge, pieceB.NorthEdge);
                case Direction.EAST:
                    return EdgePatternMatches(pieceA.EastEdge, pieceB.WestEdge);
                case Direction.WEST:
                    return EdgePatternMatches(pieceA.WestEdge, pieceB.EastEdge);
                default:
                    return false;
            }
        }

        public bool EdgePatternMatches(PieceData otherPiece, Direction edgeOnThis) {
            return EdgePatternMatches(this, otherPiece, edgeOnThis);
        }

    }
}
