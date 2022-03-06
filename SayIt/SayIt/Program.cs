using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using NAudio.Wave;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SayIt {
    public class Program {

        public class HashTriple {
            public ulong AvgHash { get; }
            public ulong DiffHash { get; }
            public ulong PerceptHash { get; }
            public HashTriple(ulong avg, ulong diff, ulong percept) {
                this.AvgHash = avg;
                this.DiffHash = diff;
                this.PerceptHash = percept;
            }
        }

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private const string PATH_TO_MP3 = "sayIt.mp3";
        private const int BUFFER_SIZE = 10240;
        private const int FFT_WINDOW_SIZE = 2048;
        private const int FFT_STEP_SIZE = 100;
        private const int FFT_MIN_FREQUENCY = 50;
        private const int FFT_MAX_FREQUENCY = 2000;
        private const int FFT_EXPORT_MULTIPLIER = 65536;
        private const int SPLIT_SILENCE_DISTANCE = 1500;
        private const string SPECTRUM_FILE_PREFIX = "spectrum";
        private const int CUT_THRESHOULD_WIDTH = 10;
        private const int CUT_THRESHOLD = 5000;

        private const int RUN_X_PERCENT = 10;
        private const int BUCKET_HASH_MATCH_THRESHOLD = 88;

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

            private readonly AverageHash averageHash = new AverageHash();
            private readonly DifferenceHash differenceHash = new DifferenceHash();
            private readonly PerceptualHash perceptualHash = new PerceptualHash();

            public List<string> CreateSpectograms(string audioFilePath) {
                LOGGER.Info("Reading audio file.");
                List<string> result = new List<string>();

                using (AudioFileReader audioFile = new AudioFileReader(audioFilePath)) {
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
                            LOGGER.Info("Creating spectrograms .. completed {per,3}% .. saving image.", percentageDone);

                            // make sure the spectrogram ends during a pause (except at 100%)
                            int splitIndex = (percentageDone == 100) ? audioData.Count : DetermineSplitIndex(audioData);

                            result.Add(SaveIndexedSpectogram(audioData, audioFile.WaveFormat.SampleRate, splitIndex, percentageDone));

                            // save post-split data and make it the start of the new block
                            audioData = audioData.TakeLast(audioData.Count - splitIndex).ToList();
                        }
                    } while ((samplesRead > 0) && (percentageDone < RUN_X_PERCENT));
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

            public List<Image<Rgb24>> ExtractWordSpectrograms(List<string> spectogramFileList) {
                List<Image<Rgb24>> wordImages = new List<Image<Rgb24>>();

                foreach (string filename in spectogramFileList) {
                    LOGGER.Info("Cutting spectograms into words .. {per,3}% completed", spectogramFileList.IndexOf(filename) + 1);
                    using (Image<Rgb24> spectrogramCollection = Image.Load<Rgb24>(filename)) {
                        List<Image<Rgb24>> spectrogramList = CutIntoSingleSpectograms(spectrogramCollection);
                        wordImages.AddRange(spectrogramList);
                        //foreach (Image<Rgb24> spectrogram in spectrogramList) {
                        //    string path = string.Format("{0}_{1:D3}.png", filename[0..^4], spectrogramList.IndexOf(spectrogram) + 1);
                        //    spectrogram.SaveAsPng(path);
                        //    Image<Rgba32> spectrogram32 = spectrogram.CloneAs<Rgba32>();
                        //    spectrogram32.SaveAsPng(string.Format("{0}_{1:D3}_32.png", filename[0..^4], spectrogramList.IndexOf(spectrogram) + 1));
                        //}
                    }
                }

                return wordImages;
            }

            private static List<Image<Rgb24>> CutIntoSingleSpectograms(Image<Rgb24> spectogramCollectionImage) {
                List<Image<Rgb24>> spectogramList = new List<Image<Rgb24>>();

                int startIndex = -1;
                int endIndex = -1;
                int[] colSums = new int[CUT_THRESHOULD_WIDTH];
                for (int x = 0; x < spectogramCollectionImage.Width; x++) {
                    // current column sum
                    int colSum = 0;
                    for (int y = 0; y < spectogramCollectionImage.Height; y++) {
                        colSum += spectogramCollectionImage[x, y].R + spectogramCollectionImage[x, y].G + spectogramCollectionImage[x, y].B;
                    }
                    // determine start and end index for cut
                    if ((startIndex == -1) && (colSums.Max() < CUT_THRESHOLD) && (colSum > CUT_THRESHOLD)) {
                        startIndex = x;
                    }
                    if ((startIndex > -1) && (colSums.Max() < CUT_THRESHOLD) && (colSum < CUT_THRESHOLD)) {
                        endIndex = x - CUT_THRESHOULD_WIDTH;
                    }
                    if (x == spectogramCollectionImage.Width - 1) {
                        endIndex = x;
                    }
                    // shift column sums and add new one
                    for (int i = colSums.Length - 2; i >= 0; i--) {
                        colSums[i + 1] = colSums[i];
                    }
                    colSums[0] = colSum;
                    // cut if found
                    if ((startIndex > -1) && (endIndex > -1)) {
                        spectogramList.Add(ExtractImageSubRegion(spectogramCollectionImage, new Rectangle(startIndex, 0, endIndex - startIndex + 1, spectogramCollectionImage.Height)));
                        startIndex = -1;
                        endIndex = -1;
                    }
                }
                return spectogramList;
            }

            public List<HashTriple> CalculateHashes(List<Image<Rgb24>> wordSpectrums) {
                LOGGER.Info("Calculating hashes...");
                List<HashTriple> hashes = new List<HashTriple>();
                int wordPercentage = -1;
                int wordCount = 0;
                foreach (Image<Rgb24> word in wordSpectrums) {
                    wordCount++;
                    if (wordCount * 100 / wordSpectrums.Count > wordPercentage) {
                        wordPercentage = wordCount * 100 / wordSpectrums.Count;
                        LOGGER.Info("Hashes {per,3}% done.", wordPercentage);
                    }
                    hashes.Add(new HashTriple(averageHash.Hash(word.CloneAs<Rgba32>()), differenceHash.Hash(word.CloneAs<Rgba32>()), perceptualHash.Hash(word.CloneAs<Rgba32>())));
                }
                return hashes;
            }
        }

        static void Main(string[] args) {
            LOGGER.Info("Starting...");

            List<Tuple<List<HashTriple>, List<Image<Rgb24>>>> bucketList = new List<Tuple<List<HashTriple>, List<Image<Rgb24>>>>();

            SayItParser sayItParser = new SayItParser();

            ConsoleKeyInfo keyInfo;
            do {
                LOGGER.Warn("Process sound file?");
                keyInfo = Console.ReadKey(false);
                LOGGER.Warn("Answer: " + keyInfo.KeyChar);
            } while ((keyInfo.KeyChar != 'n') && (keyInfo.KeyChar != 'y'));

            List<string> spectralPercentList;
            if (keyInfo.KeyChar == 'y') {
                spectralPercentList = sayItParser.CreateSpectograms(PATH_TO_MP3);
            }
            else {
                spectralPercentList = Directory.GetFiles(".", SPECTRUM_FILE_PREFIX + "???.png").ToList();
                spectralPercentList = spectralPercentList.Take(spectralPercentList.Count * RUN_X_PERCENT / 100).ToList();
                spectralPercentList.Sort();
            }

            List<Image<Rgb24>> wordSpectrums = sayItParser.ExtractWordSpectrograms(spectralPercentList);
            List<HashTriple> hashes = sayItParser.CalculateHashes(wordSpectrums);

            LOGGER.Info("Matching up hashes...");
            int hashPercentage = -1;
            int hashCount = 0;
            foreach (HashTriple hash in hashes) {
                hashCount++;
                if (hashCount * 100 / hashes.Count > hashPercentage) {
                    hashPercentage = hashCount * 100 / hashes.Count;
                    LOGGER.Info("Bucket list {per,3}% done...", hashPercentage);
                }
                double bestOneTwoAvg = 0;
                int bestIndex = -1;
                foreach (var bucket in bucketList) {
                    List<double> bucketComparison = new List<double>();
                    foreach (var bucketHash in bucket.Item1) {
                        bucketComparison.Add((CompareHash.Similarity(hash.AvgHash, bucketHash.AvgHash) + CompareHash.Similarity(hash.DiffHash, bucketHash.DiffHash)) / 2);
                    }
                    if (bucketComparison.Average() > bestOneTwoAvg) {
                        bestOneTwoAvg = bucketComparison.Average();
                        bestIndex = bucketList.IndexOf(bucket);
                    }
                }
                if (bestOneTwoAvg > BUCKET_HASH_MATCH_THRESHOLD) {
                    bucketList[bestIndex].Item1.Add(hash);
                    bucketList[bestIndex].Item2.Add(wordSpectrums[hashes.IndexOf(hash)]);
                }
                else {
                    bucketList.Add(new Tuple<List<HashTriple>, List<Image<Rgb24>>>(new List<HashTriple>(new HashTriple[] { hash }), new List<Image<Rgb24>>(new Image<Rgb24>[] { wordSpectrums[hashes.IndexOf(hash)] })));
                }
            }
            LOGGER.Info("Found {} different buckets.", bucketList.Count);

            LOGGER.Info("Saving buckets...");
            Directory.CreateDirectory("buckets");
            foreach (var bucket in bucketList) {
                string bucketName = "bucket" + (bucketList.IndexOf(bucket) + 1);
                Directory.CreateDirectory("buckets/" + bucketName);
                foreach (var word in bucket.Item2) {
                    string entryName = "word" + (bucket.Item2.IndexOf(word) + 1) + ".png";
                    word.SaveAsPng("buckets/" + bucketName + "/" + entryName);
                }
            }
        }

    }
}
