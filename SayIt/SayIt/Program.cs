using NAudio.Wave;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SayIt {
    public class Program {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static readonly string PATH_TO_MP3 = Path.Combine("sayIt.mp3");

        private const int BUFFER_SIZE = 10240;
        private const int FFT_WINDOW_SIZE = 2048;
        private const int FFT_STEP_SIZE = 100;
        private const int FFT_MIN_FREQUENCY = 50;
        private const int FFT_MAX_FREQUENCY = 2000;
        private const int FFT_EXPORT_MULTIPLIER = 65536;
        private const int SPLIT_SILENCE_DISTANCE = 1500;

        private const string SPECTRUM_FILE_PREFIX = "spectrum";

        // straight from libs homepage
        private static Image<Rgb24> ExtractImageSubRegion(Image<Rgb24> sourceImage, Rectangle sourceArea) {
            Image<Rgb24> targetImage = new Image<Rgb24>(sourceArea.Width, sourceArea.Height);
            int height = sourceArea.Height;
            sourceImage.ProcessPixelRows(targetImage, (sourceAccessor, targetAccessor) => {
                for (int y = 0; y < height; y++) {
                    Span<Rgb24> sourceRow = sourceAccessor.GetRowSpan(sourceArea.Y + y);
                    Span<Rgb24> targetRow = targetAccessor.GetRowSpan(y);
                    sourceRow.Slice(sourceArea.X, sourceArea.Width).CopyTo(targetRow);
                }
            });
            return targetImage;
        }

        public class SayItParser {

            public List<string> CreateSpectograms() {
                LOGGER.Info("Reading audio file.");
                List<string> result = new List<string>();

                using (AudioFileReader audioFile = new AudioFileReader(PATH_TO_MP3)) {
                    List<double> audioData = new List<double>();
                    int percentageDone = 0;
                    float[] buffer = new float[BUFFER_SIZE];
                    int samplesRead = -1;
                    do {
                        LOGGER.Debug("Reading samples to buffer (size: {buffersize})", BUFFER_SIZE);
                        samplesRead = audioFile.Read(buffer, 0, BUFFER_SIZE);
                        audioData.AddRange(buffer.Take(samplesRead).Select(x => (double)x));

                        // another percent done
                        int percentage = (int)(audioFile.Position * 100 / audioFile.Length);
                        if (percentage != percentageDone) {
                            percentageDone = percentage;
                            LOGGER.Info("Completed {per,3}% ... Saving image ...", percentageDone);

                            // make sure the spectrogram ends during a pause (except at 100%)
                            int splitIndex = (percentageDone == 100) ? audioData.Count : DetermineSplitIndex(audioData);

                            result.Add(SaveIndexedSpectogram(audioData, audioFile.WaveFormat.SampleRate, splitIndex, percentageDone));

                            // save post-split data and make it the start of the new block
                            audioData = audioData.TakeLast(audioData.Count - splitIndex).ToList();
                        }
                    } while ((samplesRead > 0) && (percentageDone < 1));
                }
                return result;
            }

            private static string SaveIndexedSpectogram(List<double> audioData, int sampleRate, int splitIndex, int fileIndex) {
                Spectrogram.SpectrogramGenerator spectrogramGenerator = new Spectrogram.SpectrogramGenerator(sampleRate, FFT_WINDOW_SIZE, FFT_STEP_SIZE, FFT_MIN_FREQUENCY, FFT_MAX_FREQUENCY);
                spectrogramGenerator.Add(audioData.Take(splitIndex));
                spectrogramGenerator.Colormap = Spectrogram.Colormap.Grayscale;
                string filename = string.Format("{0}{1:D3}.png", SPECTRUM_FILE_PREFIX, fileIndex);
                spectrogramGenerator.SaveImage(filename, FFT_EXPORT_MULTIPLIER);
                return filename;
            }

            private static int DetermineSplitIndex(List<double> audioData) {
                int splitIndex = audioData.Count;
                double preSplitSum;
                double postSplitSum;
                do {
                    splitIndex -= SPLIT_SILENCE_DISTANCE;
                    preSplitSum = audioData.GetRange(splitIndex - SPLIT_SILENCE_DISTANCE, SPLIT_SILENCE_DISTANCE).Select(x => Math.Abs(x)).Sum();
                    postSplitSum = audioData.GetRange(splitIndex, SPLIT_SILENCE_DISTANCE).Select(x => Math.Abs(x)).Sum();
                } while ((preSplitSum != 0) || (postSplitSum != 0));
                return splitIndex;
            }

            public void ExtractSignaturesFromSpectograms(List<string> spectogramFileList) {
                foreach (string filename in spectogramFileList) {
                    using (Image<Rgb24> spectrogramCollection = Image.Load<Rgb24>(filename)) {
                        CutIntoSingleSpectograms(spectrogramCollection);
                    }
                }
            }

            private static List<Image<Rgb24>> CutIntoSingleSpectograms(Image<Rgb24> spectogramCollection) {
                List<Image<Rgb24>> spectogramList = new List<Image<Rgb24>>();

                for (int x = 0; x < 250; x++) {
                    int colSum = 0;
                    for (int y = 0; y < spectogramCollection.Height; y++) {
                        colSum += spectogramCollection[x, y].R + spectogramCollection[x, y].G + spectogramCollection[x, y].B;
                    }
                    // FIXME: cutoff probably 1_000
                    // locate left and right borders
                    // cut sub-image from collection
                    // add to list
                }
                return spectogramList;
            }
        }

        static void Main(string[] args) {
            LOGGER.Info("Starting...");

            SayItParser sayItParser = new SayItParser();
            List<string> spectrogramFileList = sayItParser.CreateSpectograms();
            sayItParser.ExtractSignaturesFromSpectograms(spectrogramFileList);
        }

    }
}
