using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;

namespace AwesomeVid {
    class Program {

        private const string PATH_NUMBER_FRAMES = @"C:\Users\Yes-Man\NumberFrames";
        private const string PATH_NUMBERS = @"C:\Users\Yes-Man\Numbers";
        private const int FRAME_HEIGHT = 64;
        private const int FRAME_WIDTH = 64;
        private const int INITIAL_FRAME_SKIP = 24;
        private const int SINGLE_NUMBER_FRAME_COUNT = 16;
        private const int ONGOING_FRAME_OFFSET = 16;
        private const int MAX_FRAME_COUNT = 5000; // 365475;

        private static string CreateNumbersFilename(int i) {
            return (i < 100_000) ? string.Format("numbers-{0:D5}.jpg", i) : string.Format("numbers-{0:D6}.jpg", i);
        }

        //FIXME: change to -> add all (32) itensities; then threshold them
        private static Image<Rgb24> AddUpImageFiles(IEnumerable<Image<Rgb24>> frames) {
            Image<Rgb24> merge = new Image<Rgb24>(FRAME_WIDTH, FRAME_HEIGHT);
            foreach (Image<Rgb24> frame in frames) {
                frame.Mutate(x => x.Grayscale());
                frame.Mutate(x => x.Invert());
                frame.ProcessPixelRows(merge, (sourceAccessor, targetAccessor) => {
                    for (int y = 0; y < FRAME_HEIGHT; y++) {
                        Span<Rgb24> sourceRow = sourceAccessor.GetRowSpan(y);
                        Span<Rgb24> targetRow = targetAccessor.GetRowSpan(y);
                        for (int x = 0; x < FRAME_WIDTH; x++) {
                            ref Rgb24 sourcePixel = ref sourceRow[x];
                            if (sourcePixel.R + sourcePixel.G + sourcePixel.B > 50) {
                                targetRow[x] = sourcePixel;
                            }
                        }
                    }
                });
            }
            return merge;
        }

        static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            List<Image<Rgb24>> numberFrames = new List<Image<Rgb24>>();
            int numberCount = 0;
            bool activeFrame = false;
            for (int frameIndex = INITIAL_FRAME_SKIP + 1; frameIndex <= MAX_FRAME_COUNT; frameIndex++) {
                int relativeFramePosition = (frameIndex - INITIAL_FRAME_SKIP) % (SINGLE_NUMBER_FRAME_COUNT + ONGOING_FRAME_OFFSET);
                if (relativeFramePosition < SINGLE_NUMBER_FRAME_COUNT) {
                    activeFrame = true;
                    Console.WriteLine("Adding frame '" + CreateNumbersFilename(frameIndex) + "' to number #" + (numberCount + 1) + ".");
                    numberFrames.Add(Image.Load<Rgb24>(Path.Combine(PATH_NUMBER_FRAMES, CreateNumbersFilename(frameIndex))));
                }
                else {
                    Console.WriteLine("Skipping frame '" + CreateNumbersFilename(frameIndex) + "'.");
                    if (activeFrame) {
                        // first inactive frame
                        activeFrame = false;
                        AddUpImageFiles(numberFrames).SaveAsJpeg(Path.Combine(PATH_NUMBERS, string.Format("{0:D5}.jpg", ++numberCount)));
                        numberFrames.ForEach(x => x.Dispose());
                        numberFrames.Clear();
                    }
                }
            }

        }
    }
}
