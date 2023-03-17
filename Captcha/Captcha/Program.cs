using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Captcha {
    public class Program {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public class CaptchaSolver {

            public class CaptchaMatch {
                public int Major { get; set; }
                public int Minor { get; set; }
            }

            private static readonly int MAXIMUM_DEVIATION_FROM_BLACK = 50;
            private static readonly float MAX_NEWTON_BOUNDING_ANGLE = 16;
            private static readonly float MAX_NEWTON_CORE_ANGLE = 2;
            private static readonly float MIN_NEWTON_ANGLE = 1;
            private static readonly string PATH_TO_ORIGINAL_CAPTCHAS = Path.Combine("..", "..", "..", "captcha");
            private static readonly string PATH_TO_PREPROCESSED_CAPTCHAS = Path.Combine("..", "..", "..", "captcha_stage1");
            private static readonly string PATH_TO_CLOSE_MATCHES = Path.Combine("..", "..", "..", "matches");
            private static readonly string MATCH_RECORD_FILE = Path.Combine("..", "..", "..", "matches.xml");
            private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CaptchaMatch>));

            // top, right, bottom, left
            private static Tuple<int, int, int, int> DetermineBoundingBox(Image<Rgb24> image) {
                return DetermineSomeBox(image, MAXIMUM_DEVIATION_FROM_BLACK);
            }

            // top, right, bottom, left
            private static Tuple<int, int, int, int> DetermineCoreBox(Image<Rgb24> image) {
                return DetermineSomeBox(image, -1);
            }

            // top, right, bottom, left
            private static Tuple<int, int, int, int> DetermineSomeBox(Image<Rgb24> image, long maximumDeviation) {
                int topY = -1;
                int bottomY = -1;
                int leftX = -1;
                int rightX = -1;

                long maxDevX = maximumDeviation;
                long maxDevY = maximumDeviation;
                // no maximum deviation given; try to find word core
                if (maximumDeviation == -1) {
                    maxDevX = (image.Height / 2) * (3 * 255) / 2;
                    maxDevY = (image.Width / 2) * (3 * 255) / 2;
                }
                // top border
                for (int y = 0; y < image.Height; y++) {
                    long rowSum = 0;
                    for (int x = 0; x < image.Width; x++) {
                        Rgb24 pixel = image[x, y];
                        rowSum += pixel.R + pixel.G + pixel.B;
                    }
                    if (rowSum > maxDevY) {
                        topY = y;
                        break;
                    }
                }
                // bottom border
                for (int y = image.Height - 1; y > -1; y--) {
                    long rowSum = 0;
                    for (int x = 0; x < image.Width; x++) {
                        Rgb24 pixel = image[x, y];
                        rowSum += pixel.R + pixel.G + pixel.B;
                    }
                    if (rowSum > maxDevY) {
                        bottomY = y;
                        break;
                    }
                }
                // left border
                for (int x = 0; x < image.Width; x++) {
                    long columnSum = 0;
                    for (int y = 0; y < image.Height; y++) {
                        Rgb24 pixel = image[x, y];
                        columnSum += pixel.R + pixel.G + pixel.B;
                    }
                    if (columnSum > maxDevX) {
                        leftX = x;
                        break;
                    }
                }
                // right border
                for (int x = image.Width - 1; x > -1; x--) {
                    long columnSum = 0;
                    for (int y = 0; y < image.Height; y++) {
                        Rgb24 pixel = image[x, y];
                        columnSum += pixel.R + pixel.G + pixel.B;
                    }
                    if (columnSum > maxDevX) {
                        rightX = x;
                        break;
                    }
                }

                return new Tuple<int, int, int, int>(topY, rightX, bottomY, leftX);
            }

            private static void CropImageToBoundingBox(Image<Rgb24> image) {
                Tuple<int, int, int, int> borders = DetermineBoundingBox(image);
                int right = borders.Item2;
                int left = borders.Item4;
                int top = borders.Item1;
                int bottom = borders.Item3;

                int croppedWidth = right - left + 2;
                int croppedHeigth = bottom - top + 2;

                // crop
                LOGGER.Debug("New dimensions: " + croppedWidth + "x" + croppedHeigth);
                image.Mutate(x => x.Crop(Rectangle.FromLTRB(left, top, right, bottom)));
            }

            // use this to newton the shit out of the pictures
            private static int VirtuallyRotateAndDetermineHeight(Image<Rgb24> image, float degrees, bool limitToCoreBox) {
                Image<Rgb24> clonedImage = image.Clone();
                clonedImage.Mutate(x => x.Rotate(degrees));
                Tuple<int, int, int, int> cloneDimensions = limitToCoreBox ? DetermineCoreBox(clonedImage) : DetermineBoundingBox(clonedImage);
                return cloneDimensions.Item3 - cloneDimensions.Item1 + 1;
            }

            private static float NewtonRotationAngle(Image<Rgb24> image) {
                int currentHeight = image.Height;
                float resultingAngle = 0;
                float currentAngleChange = MAX_NEWTON_BOUNDING_ANGLE;
                bool limitToCoreBox = false;

                while (currentAngleChange >= MIN_NEWTON_ANGLE) {
                    // calculate diff in both directions
                    int trialHeight1 = VirtuallyRotateAndDetermineHeight(image, resultingAngle + currentAngleChange, limitToCoreBox);
                    int trialHeight2 = VirtuallyRotateAndDetermineHeight(image, resultingAngle - currentAngleChange, limitToCoreBox);

                    // both suck
                    if ((trialHeight1 >= currentHeight) && (trialHeight2 >= currentHeight)) {
                        currentAngleChange /= 2;
                    }
                    else {
                        // at least one is better
                        int trialHeightDifference = trialHeight1 - trialHeight2;
                        if (trialHeightDifference <= 0) {
                            resultingAngle += currentAngleChange;
                            currentHeight = trialHeight1;
                        }
                        else {
                            resultingAngle -= currentAngleChange;
                            currentHeight = trialHeight2;
                        }
                    }

                    //// first round done .. now switch to core mode
                    //if (!limitToCoreBox && (currentAngleChange < MIN_NEWTON_ANGLE)) {
                    //    limitToCoreBox = true;
                    //    currentAngleChange = MAX_NEWTON_CORE_ANGLE;
                    //}
                }
                return resultingAngle;
            }

            private static float NewtonRotationAngle_2(Image<Rgb24> image) {
                int currentHeight = image.Height;
                float resultingAngle = 0;
                float currentAngleChange = MAX_NEWTON_BOUNDING_ANGLE;
                bool limitToCoreBox = false;

                while (currentAngleChange >= MIN_NEWTON_ANGLE) {
                    LOGGER.Trace("Rotating image of height {height}.", image.Height);
                    // calculate diff in both directions
                    int trialHeight1 = VirtuallyRotateAndDetermineHeight(image, resultingAngle + currentAngleChange, limitToCoreBox);
                    LOGGER.Trace("Rotated {angle} and got a height of {height}.", resultingAngle + currentAngleChange, trialHeight1);
                    int trialHeight2 = VirtuallyRotateAndDetermineHeight(image, resultingAngle - currentAngleChange, limitToCoreBox);
                    LOGGER.Trace("Rotated {angle} and got a height of {height}.", resultingAngle - currentAngleChange, trialHeight2);

                    // both suck
                    if ((trialHeight1 >= currentHeight) && (trialHeight2 >= currentHeight)) {
                        currentAngleChange /= 2;
                        LOGGER.Trace("Rotation made it worse. Changing rotation angle to: {}", currentAngleChange);
                    }
                    else {
                        // at least one is better
                        if (trialHeight1 - trialHeight2 <= 0) {
                            LOGGER.Trace("Rotation change: {}", currentAngleChange);
                            resultingAngle += currentAngleChange;
                            currentHeight = trialHeight1;
                        }
                        else {
                            LOGGER.Trace("Rotation change: -{}", currentAngleChange);
                            resultingAngle -= currentAngleChange;
                            currentHeight = trialHeight2;
                        }
                    }

                    //// first round done .. now switch to core mode
                    //if (!limitToCoreBox && (currentAngleChange < MIN_NEWTON_ANGLE)) {
                    //    LOGGER.Trace("Switching to core box rotation.");

                    //    using FileStream fileStream = new FileStream("test_3_Original.png", FileMode.Create, FileAccess.Write);
                    //    Image<Rgb24> clonedImage = image.Clone();
                    //    clonedImage.Mutate(x => x.Rotate(resultingAngle));
                    //    clonedImage.SaveAsPng(fileStream);

                    //    limitToCoreBox = true;
                    //    currentAngleChange = MAX_NEWTON_CORE_ANGLE;
                    //}
                }
                return resultingAngle;
            }

            private static void PreProcessImages() {
                // preprocess all images
                foreach (string captchaFile in Directory.GetFiles(PATH_TO_ORIGINAL_CAPTCHAS)) {
                    LOGGER.Debug("Reading file: " + captchaFile);

                    using Image<Rgb24> originalImage = Image.Load<Rgb24>(captchaFile);
                    // initial crop
                    CropImageToBoundingBox(originalImage);

                    // newton for a rotation angle
                    string filenameOnly = Path.GetFileName(captchaFile);
                    float rotationAngle = NewtonRotationAngle(originalImage);
                    LOGGER.Warn("Angle should be: " + rotationAngle);
                    // rotate
                    originalImage.Mutate(x => x.Rotate(rotationAngle));
                    originalImage.Mutate(x => x.BinaryThreshold(0.7f, Color.White, Color.Black));

                    // crop again
                    CropImageToBoundingBox(originalImage);

                    using FileStream fileStream = new FileStream(Path.Combine(PATH_TO_PREPROCESSED_CAPTCHAS, Path.GetFileName(captchaFile)), FileMode.Create, FileAccess.Write);
                    originalImage.SaveAsPng(fileStream);
                }
            }

            private static long CalculateQuadraticImageDistance(Image<Rgb24> image1, Image<Rgb24> image2) {
                // calculate overlap
                int minWidth = Math.Min(image1.Width, image2.Width);
                int minHeight = Math.Min(image1.Height, image2.Height);
                // calculate offsets to center the images
                int offsetWidthImage1 = (image1.Width - minWidth) / 2;
                int offsetHeigthImage1 = (image1.Height - minHeight) / 2;
                int offsetWidthImage2 = (image2.Width - minWidth) / 2;
                int offsetHeigthImage2 = (image2.Height - minHeight) / 2;
                // calculate difference for centered overlap
                long distance = 0;
                for (int y = 0; y < minHeight; y++) {
                    for (int x = 0; x < minWidth; x++) {
                        Rgb24 pixelImage1 = image1[x + offsetWidthImage1, y + offsetHeigthImage1];
                        long pixelValueImage1 = pixelImage1.R + pixelImage1.G + pixelImage1.B;
                        Rgb24 pixelImage2 = image2[x + offsetWidthImage2, y + offsetHeigthImage2];
                        long pixelValueImage2 = pixelImage2.R + pixelImage2.G + pixelImage2.B;
                        // quadratic difference
                        distance += (pixelValueImage1 - pixelValueImage2) * (pixelValueImage1 - pixelValueImage2);
                    }
                }
                return distance / (minHeight * minWidth);
            }

            public void Run() {
                //LOGGER.Info("Testing rotation.");
                //// im3110.png (pseudonyms)
                //string testFile = Path.Combine(PATH_TO_ORIGINAL_CAPTCHAS, "im3110.png");
                //using Image<Rgb24> originalImage = Image.Load<Rgb24>(testFile);

                //using FileStream originalFileStream = new FileStream("test_1_Original.png", FileMode.Create, FileAccess.Write);
                //originalImage.SaveAsPng(originalFileStream);

                //// initial crop
                //CropImageToBoundingBox(originalImage);

                //using FileStream croppedFileStream = new FileStream("test_2_Cropped.png", FileMode.Create, FileAccess.Write);
                //originalImage.SaveAsPng(croppedFileStream);

                //// newton for a rotation angle
                //float rotationAngle = NewtonRotationAngle_2(originalImage);
                //// rotate
                //originalImage.Mutate(x => x.Rotate(rotationAngle));
                //originalImage.Mutate(x => x.BinaryThreshold(0.7f, Color.White, Color.Black));

                //using FileStream rotatedFileStream = new FileStream("test_4_Rotated.png", FileMode.Create, FileAccess.Write);
                //originalImage.SaveAsPng(rotatedFileStream);

                //// crop again
                //CropImageToBoundingBox(originalImage);

                //using FileStream finishedFileStream = new FileStream("test_5_Finished.png", FileMode.Create, FileAccess.Write);
                //originalImage.SaveAsPng(finishedFileStream);

                //return;

                LOGGER.Info("Captcha Solver running.");

                // make sure preprocess directory exists
                if (!Directory.Exists(PATH_TO_PREPROCESSED_CAPTCHAS)) {
                    Directory.CreateDirectory(PATH_TO_PREPROCESSED_CAPTCHAS);
                }
                // make sure match directory exists & is empty
                if (Directory.Exists(PATH_TO_CLOSE_MATCHES)) {
                    Directory.Delete(PATH_TO_CLOSE_MATCHES, true);
                }
                if (!Directory.Exists(PATH_TO_CLOSE_MATCHES)) {
                    Directory.CreateDirectory(PATH_TO_CLOSE_MATCHES);
                }

                ConsoleKeyInfo keyInfo;
                do {
                    LOGGER.Warn("Preprocess images?");
                    keyInfo = Console.ReadKey(false);
                    LOGGER.Warn("Answer: " + keyInfo.KeyChar);
                } while ((keyInfo.KeyChar != 'n') && (keyInfo.KeyChar != 'y'));

                if (keyInfo.KeyChar == 'y') {
                    PreProcessImages();
                }

                string maxSimilarityString;
                long maxSimilarity;
                do {
                    LOGGER.Warn("Maximum result similarity (100k will find hits; increasing will find more):");
                    maxSimilarityString = Console.ReadLine();
                } while (!long.TryParse(maxSimilarityString, out maxSimilarity));

                // read all images into memory (~100MB)
                string[] fileNames = Directory.GetFiles(PATH_TO_PREPROCESSED_CAPTCHAS);
                int fileCount = fileNames.Length;
                KeyValuePair<string, Image<Rgb24>>[] completeImageData = new KeyValuePair<string, Image<Rgb24>>[fileCount];
                for (int imageIndex = 0; imageIndex < fileCount; imageIndex++) {
                    completeImageData[imageIndex] = new KeyValuePair<string, Image<Rgb24>>(fileNames[imageIndex], Image.Load<Rgb24>(fileNames[imageIndex]));
                }

                // build similarity matrix & record matches
                long[,] similarityMatrix = new long[fileCount, fileCount];
                List<CaptchaMatch> recordedMatches = new List<CaptchaMatch>();

                for (int major = 0; major < fileCount - 2; major++) {
                    if (major % 500 == 0) {
                        LOGGER.Info("Major: " + major);
                    }
                    for (int minor = major + 1; minor < fileCount; minor++) {
                        Image<Rgb24> majorImage = completeImageData[major].Value;
                        Image<Rgb24> minorImage = completeImageData[minor].Value;
                        if (Math.Abs(majorImage.Width - minorImage.Width) >= 3) {
                            similarityMatrix[major, minor] = long.MaxValue;
                        }
                        else {
                            similarityMatrix[major, minor] = CalculateQuadraticImageDistance(majorImage, minorImage);
                        }
                        if (similarityMatrix[major, minor] < maxSimilarity) {
                            LOGGER.Info($"{Path.GetFileName(completeImageData[major].Key),11} vs {Path.GetFileName(completeImageData[minor].Key),11} : {similarityMatrix[major, minor]}");
                            Guid guid = Guid.NewGuid();
                            File.Copy(completeImageData[major].Key, Path.Combine(PATH_TO_CLOSE_MATCHES, guid + "_" + Path.GetFileName(completeImageData[major].Key)));
                            File.Copy(completeImageData[minor].Key, Path.Combine(PATH_TO_CLOSE_MATCHES, guid + "_" + Path.GetFileName(completeImageData[minor].Key)));
                            recordedMatches.Add(new CaptchaMatch() { Major = major, Minor = minor });
                        }
                    }
                }

                using FileStream fileStream = new FileStream(MATCH_RECORD_FILE, FileMode.Create, FileAccess.Write);
                xmlSerializer.Serialize(fileStream, recordedMatches);
            }

        }

        static void Main(string[] args) {
            LOGGER.Info("Starting...");
            CaptchaSolver solver = new CaptchaSolver();
            solver.Run();
        }

    }
}
