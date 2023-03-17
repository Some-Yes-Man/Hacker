using NLog;

namespace ShatterThosePictures {
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

        private static int ID_GENERATOR = 0;

        public int Id { get; private set; }
        public byte NorthEdge { get; set; }
        public byte SouthEdge { get; set; }
        public byte EastEdge { get; set; }
        public byte WestEdge { get; set; }
        public HashSet<Direction> PuzzleEdges { get; set; }
        public Color[,] ImageData { get; set; }
        public string Filename { get; set; }

        public PuzzlePiece() {
            this.PuzzleEdges = new HashSet<Direction>() { };
            this.ImageData = new Color[DATA_WIDTH, DATA_HEIGHT];
            this.Filename = string.Empty;
        }

        public PuzzlePiece(string filename) {
            this.Id = ++ID_GENERATOR;
            this.Filename = filename;
            this.PuzzleEdges = new HashSet<Direction>();
            this.ImageData = new Color[DATA_WIDTH, DATA_HEIGHT];

            // get initial pixel & data area
            LOGGER.Trace("Reading '{filename}'.", filename);
            Image<Rgb24> image = Image.Load<Rgb24>(filename);

            Rgb24 backgroundColor = new();
            image.ProcessPixelRows(accessor => {
                Span<Rgb24> row = accessor.GetRowSpan(0);
                ref Rgb24 pixel = ref row[0];
                backgroundColor.FromRgb24(pixel);
            });

            byte startX = 0;
            byte startY = 0;
            while (!BitmapColIsCore(image, startX, backgroundColor)) {
                startX++;
            }
            while (!BitmapRowIsCore(image, startY, backgroundColor)) {
                startY++;
            }
            // remove bit length from starting positions to catch outer bits
            startX -= BIT_LENGTH;
            startY -= BIT_LENGTH;
            LOGGER.Trace("Found image data at {x}:{y}.", startX, startY);

            // get image data
            image.ProcessPixelRows(accessor => {
                for (byte y = startY; y < (startY + DATA_HEIGHT); y++) {
                    Span<Rgb24> row = accessor.GetRowSpan(y);
                    for (byte x = startX; x < (startX + DATA_WIDTH); x++) {
                        ref Rgb24 pixel = ref row[x];
                        this.ImageData[x - startX, y - startY] = pixel.Equals(backgroundColor) ? Color.Transparent : new Color(pixel);
                    }
                }
            });

            // EDGES
            byte northInnerEdge = 0;
            for (int n = 0; n < BIT_COUNT; n++) {
                Color outerBitPixelColor = this.ImageData[BIT_OFFSET + n * (BIT_THICCNESS + BIT_SPACING), 0];
                Color innerBitPixelColor = this.ImageData[BIT_OFFSET + n * (BIT_THICCNESS + BIT_SPACING), BIT_LENGTH];
                if (!Color.Transparent.Equals(outerBitPixelColor)) {
                    this.NorthEdge += (byte)Math.Pow(2, n);
                }
                if (!Color.Transparent.Equals(innerBitPixelColor)) {
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
                if (!Color.Transparent.Equals(outerBitPixelColor)) {
                    this.SouthEdge += (byte)Math.Pow(2, s);
                }
                if (!Color.Transparent.Equals(innerBitPixelColor)) {
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
                if (!Color.Transparent.Equals(outerBitPixelColor)) {
                    this.EastEdge += (byte)Math.Pow(2, e);
                }
                if (!Color.Transparent.Equals(innerBitPixelColor)) {
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
                if (!Color.Transparent.Equals(outerBitPixelColor)) {
                    this.WestEdge += (byte)Math.Pow(2, w);
                }
                if (!Color.Transparent.Equals(innerBitPixelColor)) {
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

        public override bool Equals(object? obj) {
            if (obj is not PuzzlePiece other) {
                return false;
            }
            return this.Id.Equals(other.Id);
        }

        public override string ToString() {
            return this.Id.ToString();
        }

        private static bool BitmapRowIsCore(Image<Rgb24> image, byte y, Rgb24 backgroundColor) {
            byte consecutiveDataPixels = 0;
            bool isCore = false;
            image.ProcessPixelRows(accesssor => {
                Span<Rgb24> row = accesssor.GetRowSpan(y);
                for (byte x = 0; x < image.Width; x++) {
                    ref Rgb24 pixel = ref row[x];
                    if (!pixel.Equals(backgroundColor)) {
                        consecutiveDataPixels++;
                    }
                    else {
                        consecutiveDataPixels = 0;
                    }
                    if (consecutiveDataPixels == 3) {
                        isCore = true;
                        return;
                    }
                }
            });
            return isCore;
        }

        private static bool BitmapColIsCore(Image<Rgb24> image, byte x, Rgb24 backgroundColor) {
            byte consecutiveDataPixels = 0;
            bool isCore = false;
            image.ProcessPixelRows(accessor => {
                for (byte y = 0; y < image.Height; y++) {
                    Span<Rgb24> row = accessor.GetRowSpan(y);
                    ref Rgb24 pixel = ref row[x];
                    if (!pixel.Equals(backgroundColor)) {
                        consecutiveDataPixels++;
                    }
                    else {
                        consecutiveDataPixels = 0;
                    }
                    if (consecutiveDataPixels == 3) {
                        isCore = true;
                        return;
                    }
                }
            });
            return isCore;
        }

    }
}
