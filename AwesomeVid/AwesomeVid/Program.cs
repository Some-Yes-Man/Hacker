using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using AwesomeVid.Properties;
using System.Linq;

namespace AwesomeVid {
    class Program {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private const string PATH_NUMBER_FRAMES = @"C:\Users\robert.krausse\Downloads\NumberFrames";
        private const string PATH_NUMBERS = @"C:\Users\robert.krausse\Downloads\Numbers";
        private const int FRAME_HEIGHT = 64;
        private const int FRAME_WIDTH = 64;
        private const int FRAME_SKIP_FRONT = 8;
        private const int FRAME_CONTENT = 14;
        private const double FRAMES_PER_NUMBER = 365475.0 / 2 / 5624;
        private const int MAX_FRAME_COUNT = 365475;

        private static readonly Image<Rgb24>[] referenceChars = new Image<Rgb24>[16] {
            Image.Load<Rgb24>(Resources._0), Image.Load<Rgb24>(Resources._1), Image.Load<Rgb24>(Resources._2), Image.Load<Rgb24>(Resources._3),
            Image.Load<Rgb24>(Resources._4), Image.Load<Rgb24>(Resources._5), Image.Load<Rgb24>(Resources._6), Image.Load<Rgb24>(Resources._7),
            Image.Load<Rgb24>(Resources._8), Image.Load<Rgb24>(Resources._9), Image.Load<Rgb24>(Resources.a), Image.Load<Rgb24>(Resources.b),
            Image.Load<Rgb24>(Resources.c), Image.Load<Rgb24>(Resources.d), Image.Load<Rgb24>(Resources.e), Image.Load<Rgb24>(Resources.f) };
        private static readonly List<byte> output = new List<byte>();

        enum FrameState {
            UNKNOWN,
            FRONT,
            CONTENT,
            BACK
        }

        private static string CreateNumbersFilename(int i) {
            return (i < 100_000) ? string.Format("numbers-{0:D5}.jpg", i) : string.Format("numbers-{0:D6}.jpg", i);
        }

        private static Image<Rgb24> AddUpImageFiles(IEnumerable<Image<Rgb24>> frames) {
            Image<Rgb24> merge = new Image<Rgb24>(FRAME_WIDTH, FRAME_HEIGHT);
            foreach (Image<Rgb24> frame in frames) {
                frame.Mutate(x => x.Grayscale());
                frame.Mutate(x => x.AdaptiveThreshold());
                frame.Mutate(x => x.Invert());
                frame.ProcessPixelRows(merge, (sourceAccessor, targetAccessor) => {
                    for (int y = 0; y < FRAME_HEIGHT; y++) {
                        Span<Rgb24> sourceRow = sourceAccessor.GetRowSpan(y);
                        Span<Rgb24> targetRow = targetAccessor.GetRowSpan(y);
                        for (int x = 0; x < FRAME_WIDTH; x++) {
                            ref Rgb24 sourcePixel = ref sourceRow[x];
                            ref Rgb24 targetPixel = ref targetRow[x];
                            if ((sourcePixel.R > 30) && (sourcePixel.R > targetPixel.R)) {
                                targetRow[x] = sourcePixel;
                            }
                        }
                    }
                });
            }
            return merge;
        }

        private static byte getMostLikelyChar(Image<Rgb24> image) {
            double minValue = double.MaxValue;
            int minIndex = -1;
            for (int i = 0; i < 16; i++) {
                double min = diffImages(image, referenceChars[i]);
                if (min < minValue) {
                    minValue = min;
                    minIndex = i;
                }
            }
            LOGGER.Debug("Image is most likely showing a '{index}'.", minIndex.ToString("X"));
            return (byte)minIndex;
        }

        private static double diffImages(Image<Rgb24> image1, Image<Rgb24> image2) {
            double diff = 0;
            image1.ProcessPixelRows(image2, (one, two) => {
                for (int y = 0; y < FRAME_HEIGHT; y++) {
                    Span<Rgb24> spanOne = one.GetRowSpan(y);
                    Span<Rgb24> spanTwo = two.GetRowSpan(y);
                    for (int x = 0; x < FRAME_WIDTH; x++) {
                        ref Rgb24 pixelOne = ref spanOne[x];
                        ref Rgb24 pixelTwo = ref spanTwo[x];
                        diff += Math.Abs(pixelOne.R - pixelTwo.R);
                    }
                }
            });
            return diff / (FRAME_WIDTH * FRAME_HEIGHT) / 255;
        }

        static void Main(string[] args) {
            LOGGER.Info("Hello World!");

            List<Image<Rgb24>> numberFrames = new List<Image<Rgb24>>();
            FrameState state = FrameState.FRONT;
            int number = 1;
            int skippedFrames = 0;
            int contentFrames = 0;
            byte upperByte = 0;
            for (int frameIndex = 1; frameIndex <= MAX_FRAME_COUNT; frameIndex++) {
                if (frameIndex % 10000 == 0) {
                    LOGGER.Info("Processing frame #{}...", frameIndex);
                }
                int currentNumber = (int)Math.Ceiling(frameIndex / FRAMES_PER_NUMBER);
                if (currentNumber > number) {

                    Image<Rgb24> addedImage = AddUpImageFiles(numberFrames);
                    numberFrames.ForEach(x => x.Dispose());
                    numberFrames.Clear();
                    byte likelyChar = getMostLikelyChar(addedImage);
                    if (number % 2 == 1) {
                        upperByte = likelyChar;
                    }
                    else {
                        output.Add((byte)(upperByte * 16 + likelyChar));
                    }
                    //addedImage.SaveAsJpeg(Path.Combine(PATH_NUMBERS, string.Format("{0:D5}.jpg", number)));

                    number = currentNumber;
                    state = FrameState.FRONT;
                    skippedFrames = 0;
                    contentFrames = 0;
                }
                switch (state) {
                    case FrameState.FRONT:
                        LOGGER.Trace("Skipping front frame #{index}.", CreateNumbersFilename(frameIndex));
                        skippedFrames++;
                        if (skippedFrames >= FRAME_SKIP_FRONT) {
                            state = FrameState.CONTENT;
                        }
                        break;
                    case FrameState.CONTENT:
                        contentFrames++;
                        LOGGER.Trace("Adding frame #{index} to number #{number}.", CreateNumbersFilename(frameIndex), number);
                        numberFrames.Add(Image.Load<Rgb24>(Path.Combine(PATH_NUMBER_FRAMES, CreateNumbersFilename(frameIndex))));
                        if (contentFrames >= FRAME_CONTENT) {
                            state = FrameState.BACK;
                        }
                        break;
                    case FrameState.BACK:
                        LOGGER.Trace("Skipping back frame #{index}.", CreateNumbersFilename(frameIndex));
                        break;
                    default:
                        LOGGER.Warn("Unexpected frame state.");
                        break;
                }
            }

            using (FileStream fs = new FileStream("output.bmp", FileMode.Create, FileAccess.Write)) {
                fs.Write(output.ToArray());
            }
        }
    }
}
