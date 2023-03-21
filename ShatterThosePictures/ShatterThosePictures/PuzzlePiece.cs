using NLog;
using System.Collections;

namespace ShatterThosePictures {
    public class PuzzlePiece {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private enum MaskArea {
            OUTER_EDGE,
            INNER_EDGE,
            CORE
        }

        // [2 * OFFSET] + [COUNT * (THICC + SPACE)] - SPACE

        // puzzle #2
        //public const byte DATA_WIDTH = 57;
        //public const byte BIT_OFFSET = 10;
        //public const byte BIT_COUNT = 5;
        //public const byte BIT_THICCNESS = 5;
        //public const byte BIT_SPACING = 3;

        // puzzle #1
        public const byte DATA_WIDTH = 24;
        public const byte BIT_OFFSET = 5;
        public const byte BIT_COUNT = 5;
        public const byte BIT_THICCNESS = 2;
        public const byte BIT_SPACING = 1;
        public const byte BIT_LENGTH = 2;
        public const byte DATA_HEIGHT = DATA_WIDTH;

        private static readonly byte EDGE_MATCH_BYTE = (byte)(Math.Pow(2, BIT_COUNT) - 1);
        private static int ID_GENERATOR = 0;

        public int Id { get; private set; }

        private BitArray north = new(1);
        private BitArray south = new(1);
        private BitArray east = new(1);
        private BitArray west = new(1);
        private byte northEdge;
        private byte southEdge;
        private byte eastEdge;
        private byte westEdge;
        public byte NorthEdge {
            get => this.northEdge;
            set {
                this.north = new BitArray(new byte[] { value });
                this.northEdge = value;
            }
        }
        public byte SouthEdge {
            get => this.southEdge;
            set {
                this.south = new BitArray(new byte[] { value });
                this.southEdge = value;
            }
        }
        public byte EastEdge {
            get => this.eastEdge;
            set {
                this.east = new BitArray(new byte[] { value });
                this.eastEdge = value;
            }
        }
        public byte WestEdge {
            get => this.westEdge;
            set {
                this.west = new BitArray(new byte[] { value });
                this.westEdge = value;
            }
        }

        public HashSet<Direction> PuzzleEdges { get; set; }
        public Image<Rgba32> ImageData { get; set; }
        public string Filename { get; set; }

        private static bool BitmapRowIsCore(Image<Rgba32> image, byte y, Rgba32 backgroundColor) {
            byte consecutiveDataPixels = 0;
            bool isCore = false;
            image.ProcessPixelRows(accesssor => {
                Span<Rgba32> row = accesssor.GetRowSpan(y);
                for (byte x = 0; x < image.Width; x++) {
                    ref Rgba32 pixel = ref row[x];
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

        private static bool BitmapColIsCore(Image<Rgba32> image, byte x, Rgba32 backgroundColor) {
            byte consecutiveDataPixels = 0;
            bool isCore = false;
            image.ProcessPixelRows(accessor => {
                for (byte y = 0; y < image.Height; y++) {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    ref Rgba32 pixel = ref row[x];
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

        public PuzzlePiece() {
            this.PuzzleEdges = new HashSet<Direction>() { };
            this.ImageData = new(DATA_WIDTH, DATA_HEIGHT, Color.Transparent);
            this.Filename = string.Empty;
        }

        public PuzzlePiece(string filename) {
            this.Id = ++ID_GENERATOR;
            this.Filename = filename;
            this.PuzzleEdges = new HashSet<Direction>();
            this.ImageData = new(DATA_WIDTH, DATA_HEIGHT, Color.Transparent);

            // get initial pixel & data area
            Logger.Trace("Reading '{filename}'.", filename);
            Image<Rgba32> image = Image.Load<Rgba32>(filename);

            Rgba32 backgroundColor = new();
            image.ProcessPixelRows(accessor => {
                Span<Rgba32> row = accessor.GetRowSpan(0);
                ref Rgba32 pixel = ref row[0];
                backgroundColor.FromRgba32(pixel);
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
            Logger.Trace("Found image data at {x}:{y}.", startX, startY);

            // get image data
            image.ProcessPixelRows(accessor => {
                for (byte y = startY; y < (startY + DATA_HEIGHT); y++) {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (byte x = startX; x < (startX + DATA_WIDTH); x++) {
                        ref Rgba32 pixel = ref row[x];
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

        public void LoadData(Image<Rgba32> image, int pieceOffsetX, int pieceOffsetY) {
            image.ProcessPixelRows(accessor => {
                int startY = pieceOffsetY * (DATA_HEIGHT - 2 * BIT_LENGTH);
                int endY = pieceOffsetY * (DATA_HEIGHT - 2 * BIT_LENGTH) + DATA_HEIGHT;

                for (int y = startY; y < endY; y++) {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    int startX = pieceOffsetX * (DATA_WIDTH - 2 * BIT_LENGTH);
                    int endX = pieceOffsetX * (DATA_WIDTH - 2 * BIT_LENGTH) + DATA_WIDTH;

                    for (int x = startX; x < endX; x++) {
                        ref Rgba32 pixel = ref row[x];
                        this.ImageData[x - startX, y - startY] = new Color(pixel);
                    }
                }
            });
            MaskData();
        }

        private void MaskData() {
            MaskArea areaX = MaskArea.OUTER_EDGE;
            MaskArea areaY = MaskArea.OUTER_EDGE;

            for (int y = 0; y < DATA_HEIGHT; y++) {
                if (((0 <= y) && (y < BIT_LENGTH)) || ((DATA_HEIGHT - BIT_LENGTH <= y) && (y < DATA_HEIGHT))) {
                    areaY = MaskArea.OUTER_EDGE;
                }
                if (((BIT_LENGTH <= y) && (y < BIT_LENGTH * 2)) || ((DATA_HEIGHT - BIT_LENGTH * 2 <= y) && (y < DATA_HEIGHT - BIT_LENGTH))) {
                    areaY = MaskArea.INNER_EDGE;
                }
                if ((BIT_LENGTH * 2 <= y) && (y < DATA_HEIGHT - BIT_LENGTH * 2)) {
                    areaY = MaskArea.CORE;
                }

                for (int x = 0; x < DATA_WIDTH; x++) {
                    if (((0 <= x) && (x < BIT_LENGTH)) || ((DATA_HEIGHT - BIT_LENGTH <= x) && (x < DATA_HEIGHT))) {
                        areaX = MaskArea.OUTER_EDGE;
                    }
                    if (((BIT_LENGTH <= x) && (x < BIT_LENGTH * 2)) || ((DATA_HEIGHT - BIT_LENGTH * 2 <= x) && (x < DATA_HEIGHT - BIT_LENGTH))) {
                        areaX = MaskArea.INNER_EDGE;
                    }
                    if ((BIT_LENGTH * 2 <= x) && (x < DATA_HEIGHT - BIT_LENGTH * 2)) {
                        areaX = MaskArea.CORE;
                    }

                    switch (areaY) {
                        case MaskArea.OUTER_EDGE:
                            if ((areaX == MaskArea.OUTER_EDGE) || (areaX == MaskArea.INNER_EDGE)) {
                                this.ImageData[x, y] = Color.Transparent;
                            }
                            if (areaX == MaskArea.CORE) {
                                // start + end offset
                                if ((x < BIT_OFFSET) || (x >= DATA_WIDTH - BIT_OFFSET)) {
                                    this.ImageData[x, y] = Color.Transparent;
                                }
                                else {
                                    int relX = x - BIT_OFFSET;
                                    // spaces
                                    if (relX % (BIT_THICCNESS + BIT_SPACING) >= BIT_THICCNESS) {
                                        this.ImageData[x, y] = Color.Transparent;
                                    }
                                    // bits
                                    else {
                                        BitArray northSouthEdge = (y < BIT_LENGTH) ? north : south;
                                        if (!northSouthEdge[relX / (BIT_THICCNESS + BIT_SPACING)]) {
                                            this.ImageData[x, y] = Color.Transparent;
                                        }
                                    }
                                }
                            }
                            break;
                        case MaskArea.INNER_EDGE:
                            if (areaX == MaskArea.OUTER_EDGE) {
                                this.ImageData[x, y] = Color.Transparent;
                            }
                            if (areaX == MaskArea.INNER_EDGE) {
                                // no masking needed
                            }
                            if (areaX == MaskArea.CORE) {
                                int relX = x - BIT_OFFSET;
                                // bits
                                if ((x >= BIT_OFFSET) && (x < DATA_WIDTH - BIT_OFFSET) && (relX % (BIT_THICCNESS + BIT_SPACING) < BIT_THICCNESS)) {
                                    BitArray northSouthEdge = (y < BIT_LENGTH * 2) ? north : south;
                                    if (!northSouthEdge[relX / (BIT_THICCNESS + BIT_SPACING)]) {
                                        this.ImageData[x, y] = Color.Transparent;
                                    }
                                }
                            }
                            break;
                        case MaskArea.CORE:
                            if (areaX == MaskArea.OUTER_EDGE) {
                                // start + end offset
                                if ((y < BIT_OFFSET) || (y >= DATA_WIDTH - BIT_OFFSET)) {
                                    this.ImageData[x, y] = Color.Transparent;
                                }
                                else {
                                    int relY = y - BIT_OFFSET;
                                    // spaces
                                    if (relY % (BIT_THICCNESS + BIT_SPACING) >= BIT_THICCNESS) {
                                        this.ImageData[x, y] = Color.Transparent;
                                    }
                                    // bits
                                    else {
                                        BitArray westEastEdge = (x < BIT_LENGTH) ? west : east;
                                        if (!westEastEdge[relY / (BIT_THICCNESS + BIT_SPACING)]) {
                                            this.ImageData[x, y] = Color.Transparent;
                                        }
                                    }
                                }
                            }
                            if (areaX == MaskArea.INNER_EDGE) {
                                int relY = y - BIT_OFFSET;
                                // bits
                                if ((y >= BIT_OFFSET) && (y < DATA_HEIGHT - BIT_OFFSET) && (relY % (BIT_THICCNESS + BIT_SPACING) < BIT_THICCNESS)) {
                                    BitArray westEastEdge = (x < BIT_LENGTH * 2) ? west : east;
                                    if (!westEastEdge[relY / (BIT_THICCNESS + BIT_SPACING)]) {
                                        this.ImageData[x, y] = Color.Transparent;
                                    }
                                }
                            }
                            if (areaX == MaskArea.CORE) {
                                // no masking needed
                            }
                            break;
                    }
                }
            }
        }

        public void SaveToFile(string filename) {
            using Image<Rgba32> image = new(DATA_WIDTH, DATA_HEIGHT, Color.Transparent);
            image.ProcessPixelRows(accessor => {
                for (int y = 0; y < DATA_HEIGHT; y++) {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < DATA_WIDTH; x++) {
                        ref Rgba32 pixel = ref row[x];
                        pixel = this.ImageData[x, y];
                    }
                }
            });
            image.Save(filename);
        }

        private static bool EdgeMatches(byte edgeA, byte edgeB) {
            return (edgeA ^ edgeB) == EDGE_MATCH_BYTE;
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
                    Logger.Warn("Unknown direction used for edge matching.");
                    return false;
            }
        }

        public static void SaveTrainingData(PuzzlePiece pieceA, PuzzlePiece pieceB, Direction edgeOnA, string filename) {
            Image<Rgba32> trainingImage = new(DATA_WIDTH, 4 * BIT_LENGTH, Color.Transparent);
            Image<Rgba32> imageA = pieceA.ImageData.Clone();
            Image<Rgba32> imageB = pieceB.ImageData.Clone();
            switch (edgeOnA) {
                case Direction.NORTH:
                    // no rotation needed
                    break;
                case Direction.SOUTH:
                    imageA.Mutate(x => x.Rotate(RotateMode.Rotate180));
                    imageB.Mutate(x => x.Rotate(RotateMode.Rotate180));
                    break;
                case Direction.EAST:
                    imageA.Mutate(x => x.Rotate(RotateMode.Rotate270));
                    imageB.Mutate(x => x.Rotate(RotateMode.Rotate270));
                    break;
                case Direction.WEST:
                    imageA.Mutate(x => x.Rotate(RotateMode.Rotate90));
                    imageB.Mutate(x => x.Rotate(RotateMode.Rotate90));
                    break;
                case Direction.UNKNOWN:
                default:
                    Logger.Warn("Unknown direction while saving training data.");
                    break;
            }

            imageA.ProcessPixelRows(trainingImage, (accessA, accessTrain) => {
                for (int y = 0; y < 2 * BIT_LENGTH; y++) {
                    Span<Rgba32> row = accessA.GetRowSpan(y);
                    row.CopyTo(accessTrain.GetRowSpan(2 * BIT_LENGTH + y));
                }
            });
            imageB.ProcessPixelRows(trainingImage, (accessB, accessTrain) => {
                for (int y = 0; y < 2 * BIT_LENGTH; y++) {
                    Span<Rgba32> row = accessB.GetRowSpan((DATA_HEIGHT - 2 * BIT_LENGTH) + y);
                    row.CopyTo(accessTrain.GetRowSpan(y));
                }
            });
            trainingImage.Save(filename);
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

    }
}
