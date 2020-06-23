using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SpiralBits {
    public partial class FormSpiralBits : Form {

        const int START_X = 434;
        const int START_Y = 36;
        const int STEP_SIZE = 3;
        const double AVERAGE_STEP_SIZE = 2.68281938;

        private LinkedList<Tuple<bool, Tuple<int, int>>> bitValues = new LinkedList<Tuple<bool, Tuple<int, int>>>();
        private Bitmap spiralBitsBitmap;
        private Bitmap spiralPathBitmap;
        private BackgroundWorker bgWorker = new BackgroundWorker();
        private int overallStepsTaken = 0;
        private LinkedList<Tuple<int, int>> gaps = new LinkedList<Tuple<int, int>>();

        private Tuple<int, int> currentPosition = new Tuple<int, int>(START_X, START_Y);
        private Tuple<int, int> previousPosition = new Tuple<int, int>(START_X, START_Y);
        private Tuple<int, int> nextPosition;

        public FormSpiralBits() {
            InitializeComponent();
        }

        public enum BitValue {
            BRIGHT,
            DARK,
            UNKNOWN
        }

        public enum Direction {
            N = 0, NE = 1, E = 2, SE = 3, S = 4, SW = 5, W = 6, NW = 7
        }

        private static BitValue DetermineValueFromColor(Color color) {
            if ((color.R < 40) && (color.G > 80) && (color.G < 160) && (color.B > 50) && (color.B < 130)) {
                return BitValue.DARK;
            }
            if ((color.R > 160) && (color.R <= 255) && (color.G > 160) && (color.G <= 255) && (color.B > 160) && (color.B <= 255)) {
                return BitValue.BRIGHT;
            }
            return BitValue.UNKNOWN;
        }

        private static bool GuestimateBitFromArea(Bitmap bitmap, int x, int y) {
            BitValue[] areaBits = new BitValue[ 9 ];
            // get bits from surrounding area
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    areaBits[ (i + 1) * 3 + (j + 1) ] = DetermineValueFromColor(bitmap.GetPixel(x + i, y + j));
                }
            }
            // determine mayority color
            return (areaBits.Count(bit => bit == BitValue.BRIGHT) >= areaBits.Count(bit => bit == BitValue.DARK)) ? false : true;
        }

        private static BitValue GuestimateValueFromArea(Bitmap bitmap, int x, int y) {
            BitValue[] areaBits = new BitValue[ 9 ];
            // get bits from surrounding area
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    areaBits[ (i + 1) * 3 + (j + 1) ] = DetermineValueFromColor(bitmap.GetPixel(x + i, y + j));
                }
            }
            if ((Math.Abs(areaBits.Count(bit => bit == BitValue.BRIGHT) - areaBits.Count(bit => bit == BitValue.DARK)) <= 2) || (areaBits.Count(bit => bit == BitValue.UNKNOWN) >= 5)) {
                return BitValue.UNKNOWN;
            }
            return (areaBits.Count(bit => bit == BitValue.BRIGHT) >= areaBits.Count(bit => bit == BitValue.DARK)) ? BitValue.BRIGHT : BitValue.DARK;
        }

        private Tuple<int, int> GetNextStep(Tuple<int, int> currentPosition, Tuple<int, int> previousPosition) {
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if ((x == 0) && (y == 0)) {
                        continue;
                    }
                    if (((currentPosition.Item1 + x != previousPosition.Item1) || (currentPosition.Item2 + y != previousPosition.Item2))
                        && (this.spiralPathBitmap.GetPixel(currentPosition.Item1 + x, currentPosition.Item2 + y).R > 100)) {
                        return new Tuple<int, int>(currentPosition.Item1 + x, currentPosition.Item2 + y);
                    }
                }
            }
            return new Tuple<int, int>(0, 0);
        }

        public static byte[] ToByteArray(BitArray bits) {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0)
                numBytes++;

            byte[] bytes = new byte[ numBytes ];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++) {
                if (bits[ i ])
                    bytes[ byteIndex ] |= (byte) (1 << (7 - bitIndex));

                bitIndex++;
                if (bitIndex == 8) {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        private void Form1_Load(object sender, EventArgs e) {
            this.spiralBitsBitmap = new Bitmap(picBox.Image);
            this.spiralPathBitmap = new Bitmap(Properties.Resources.spiral2);

            this.bgWorker = new BackgroundWorker();
            this.bgWorker.DoWork += BgWorker_DoWork;
            this.bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            this.bgWorker.RunWorkerAsync();
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e) {
            //this.bitValues.AddLast(new Tuple<bool, Tuple<int, int>>(GuestimateBitFromArea(this.spiralBitsBitmap, currentPosition.Item1, currentPosition.Item2),
            //    new Tuple<int, int>(currentPosition.Item1, currentPosition.Item2)));

            //BitValue previousBitValue = GuestimateValueFromArea(this.spiralBitsBitmap, currentPosition.Item1, currentPosition.Item2);
            //BitValue currentBitValue;
            //bool currentlyOnBorder = false;

            //while (this.currentPosition.Item1 != 0) {
            //    this.nextPosition = GetNextStep(this.currentPosition, this.previousPosition);
            //    this.previousPosition = this.currentPosition;
            //    this.currentPosition = this.nextPosition;

            //    if (this.currentPosition.Item1 == 0) {
            //        break;
            //    }

            //    currentBitValue = GuestimateValueFromArea(this.spiralBitsBitmap, currentPosition.Item1, currentPosition.Item2);
            //    this.overallStepsTaken++;

            //    if (currentlyOnBorder) {
            //        switch (currentBitValue) {
            //            case BitValue.BRIGHT:
            //            case BitValue.DARK:
            //                this.bitValues.AddLast(new Tuple<bool, Tuple<int, int>>(currentBitValue == BitValue.BRIGHT ? true : false, new Tuple<int, int>(currentPosition.Item1, currentPosition.Item2)));
            //                previousBitValue = currentBitValue;
            //                currentlyOnBorder = false;
            //                break;
            //            case BitValue.UNKNOWN:
            //            default:
            //                break;
            //        }
            //    }
            //    else {
            //        switch (currentBitValue) {
            //            case BitValue.BRIGHT:
            //            case BitValue.DARK:
            //                if (currentBitValue != previousBitValue) {
            //                    this.bitValues.AddLast(new Tuple<bool, Tuple<int, int>>(currentBitValue == BitValue.BRIGHT ? true : false, new Tuple<int, int>(currentPosition.Item1, currentPosition.Item2)));
            //                    previousBitValue = currentBitValue;
            //                }
            //                break;
            //            case BitValue.UNKNOWN:
            //            default:
            //                currentlyOnBorder = true;
            //                break;
            //        }
            //    }
            //}

            //return;

            switch (DetermineValueFromColor(this.spiralBitsBitmap.GetPixel(this.currentPosition.Item1, this.currentPosition.Item2))) {
                case BitValue.BRIGHT:
                    this.bitValues.AddLast(new Tuple<bool, Tuple<int, int>>(false, new Tuple<int, int>(this.currentPosition.Item1, this.currentPosition.Item2)));
                    break;
                case BitValue.DARK:
                    this.bitValues.AddLast(new Tuple<bool, Tuple<int, int>>(true, new Tuple<int, int>(this.currentPosition.Item1, this.currentPosition.Item2)));
                    break;
                case BitValue.UNKNOWN:
                    Console.WriteLine("Init failed.");
                    return;
            }
            bool foundBorder = false;
            int currentStepsTaken = 0;
            while (this.currentPosition.Item1 != 0) {
                currentStepsTaken++;
                this.overallStepsTaken++;

                // too many steps taken .. we probably missed a bit
                if (currentStepsTaken > 3) {
                    Console.WriteLine("Gap detected.");
                    this.bitValues.AddLast(new Tuple<bool, Tuple<int, int>>(GuestimateBitFromArea(this.spiralBitsBitmap, this.previousPosition.Item1, this.previousPosition.Item2),
                        new Tuple<int, int>(previousPosition.Item1, previousPosition.Item2)));
                    this.gaps.AddLast(previousPosition);
                    currentStepsTaken = 0;
                    //foundBorder = false;
                    continue;
                }

                this.nextPosition = GetNextStep(this.currentPosition, this.previousPosition);
                this.previousPosition = this.currentPosition;
                this.currentPosition = this.nextPosition;

                if (!foundBorder && (DetermineValueFromColor(this.spiralBitsBitmap.GetPixel(this.currentPosition.Item1, this.currentPosition.Item2)) == BitValue.UNKNOWN)) {
                    foundBorder = true;
                }
                else {
                    if (foundBorder) {
                        switch (DetermineValueFromColor(this.spiralBitsBitmap.GetPixel(this.currentPosition.Item1, this.currentPosition.Item2))) {
                            case BitValue.BRIGHT:
                                this.bitValues.AddLast(new Tuple<bool, Tuple<int, int>>(false, new Tuple<int, int>(this.currentPosition.Item1, this.currentPosition.Item2)));
                                foundBorder = false;
                                break;
                            case BitValue.DARK:
                                this.bitValues.AddLast(new Tuple<bool, Tuple<int, int>>(true, new Tuple<int, int>(this.currentPosition.Item1, this.currentPosition.Item2)));
                                foundBorder = false;
                                break;
                            case BitValue.UNKNOWN:
                                break;
                        }
                        currentStepsTaken = 0;
                    }
                }
            }
            this.bitValues.AddLast(new Tuple<bool, Tuple<int, int>>(false, new Tuple<int, int>(this.previousPosition.Item1 - 2, this.previousPosition.Item2 - 2)));
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            Graphics spiralGraphics = Graphics.FromImage(this.spiralBitsBitmap);
            int runningBitCount = 0;
            foreach (Tuple<bool, Tuple<int, int>> bitPoint in this.bitValues.Reverse()) {
                runningBitCount++;
                //if (runningBitCount % 32 == 0) {
                //    spiralGraphics.DrawString((runningBitCount / 8 - 1).ToString(), new Font("arial", 8), Brushes.Yellow, bitPoint.Item2.Item1, bitPoint.Item2.Item2);
                //}
                this.spiralBitsBitmap.SetPixel(bitPoint.Item2.Item1, bitPoint.Item2.Item2, bitPoint.Item1 ? Color.Yellow : Color.Magenta);
                //gap
                if (this.gaps.Any(x => (x.Item1 == bitPoint.Item2.Item1) && (x.Item2 == bitPoint.Item2.Item2))) {
                    //spiralGraphics.DrawString("O", new Font("arial", 15), Brushes.Yellow, bitPoint.Item2.Item1 - 11, bitPoint.Item2.Item2 - 11);
                    this.spiralBitsBitmap.SetPixel(bitPoint.Item2.Item1, bitPoint.Item2.Item2, bitPoint.Item1 ? Color.Yellow : Color.Red);
                }
            }
            picBox.Image = this.spiralBitsBitmap;
            Console.WriteLine("Bit count: " + this.bitValues.Count);
            Console.WriteLine("Step count: " + this.overallStepsTaken);

            // outer to inner (dark 1; bright 0)
            BitArray bits = new BitArray(this.bitValues.Count);
            LinkedListNode<Tuple<bool, Tuple<int, int>>> currentBitNode = this.bitValues.First;
            int bitCount = 0;
            while (currentBitNode != null) {
                bits[ bitCount++ ] = currentBitNode.Value.Item1;
                currentBitNode = currentBitNode.Next;
            }
            byte[] byteOutput = ToByteArray(bits);
            Console.WriteLine("outer to inner (dark 1; bright 0): " + string.Join(",", byteOutput.Select(x => x.ToString("X2"))));
            // outer to inner (dark 0; bright 1)
            Console.WriteLine("outer to inner (dark 0; bright 1): " + string.Join(",", byteOutput.Select(x => (x ^ 0xff).ToString("X2"))));
            // inner to outer (dark 1; bright 0)
            BitArray bitsReversed = new BitArray(bits.Count);
            for (int i = 0; i < bits.Count; i++) {
                bitsReversed[ bits.Count - i - 1 ] = bits[ i ];
            }
            byte[] byteOutputReversed = ToByteArray(bitsReversed);
            Console.WriteLine("outer to inner (dark 1; bright 0): " + string.Join(",", byteOutputReversed.Select(x => x.ToString("X2"))));
            // inner to outer (dark 0; bright 1); YEAH ... PNG!
            Console.WriteLine("outer to inner (dark 0; bright 1): " + string.Join(",", byteOutputReversed.Select(x => (x ^ 0xff).ToString("X2"))));

            File.WriteAllBytes("yeah.png", byteOutputReversed.Select(x => (byte) (x ^ 0xff)).ToArray());
        }

        private void picBox_Click(object sender, EventArgs e) {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK) {
                this.spiralBitsBitmap.Save(saveFileDialog1.FileName, ImageFormat.Png);
            }
        }

        private static Tuple<int, int> TakeStep(int x, int y, Direction direction) {
            switch (direction) {
                case Direction.N:
                    return new Tuple<int, int>(x, y - STEP_SIZE);
                case Direction.NE:
                    return new Tuple<int, int>(x + STEP_SIZE, y - STEP_SIZE);
                case Direction.E:
                    return new Tuple<int, int>(x + STEP_SIZE, y);
                case Direction.SE:
                    return new Tuple<int, int>(x + STEP_SIZE, y + STEP_SIZE);
                case Direction.S:
                    return new Tuple<int, int>(x, y + STEP_SIZE);
                case Direction.SW:
                    return new Tuple<int, int>(x - STEP_SIZE, y + STEP_SIZE);
                case Direction.W:
                    return new Tuple<int, int>(x - STEP_SIZE, y);
                case Direction.NW:
                    return new Tuple<int, int>(x - STEP_SIZE, y - STEP_SIZE);
                default:
                    return new Tuple<int, int>(x, y);
            }
        }

        private static Direction TakeLeftTurn(Direction direction) {
            return (Direction) (((int) direction - 1 + 8) % 8);
        }

        private static Direction TakeRightTurn(Direction direction) {
            return (Direction) (((int) direction + 1) % 8);
        }

        private static Direction DetermineDirection(Tuple<int, int> fromPos, Tuple<int, int> toPos) {
            if ((fromPos.Item1 - toPos.Item1 == 0) && (fromPos.Item2 - toPos.Item2 == STEP_SIZE)) {
                return Direction.N;
            }
            if ((fromPos.Item1 - toPos.Item1 == -STEP_SIZE) && (fromPos.Item2 - toPos.Item2 == STEP_SIZE)) {
                return Direction.NE;
            }
            if ((fromPos.Item1 - toPos.Item1 == -STEP_SIZE) && (fromPos.Item2 - toPos.Item2 == 0)) {
                return Direction.E;
            }
            if ((fromPos.Item1 - toPos.Item1 == -STEP_SIZE) && (fromPos.Item2 - toPos.Item2 == -STEP_SIZE)) {
                return Direction.SE;
            }
            if ((fromPos.Item1 - toPos.Item1 == 0) && (fromPos.Item2 - toPos.Item2 == -STEP_SIZE)) {
                return Direction.S;
            }
            if ((fromPos.Item1 - toPos.Item1 == STEP_SIZE) && (fromPos.Item2 - toPos.Item2 == -STEP_SIZE)) {
                return Direction.SW;
            }
            if ((fromPos.Item1 - toPos.Item1 == STEP_SIZE) && (fromPos.Item2 - toPos.Item2 == 0)) {
                return Direction.W;
            }
            if ((fromPos.Item1 - toPos.Item1 == STEP_SIZE) && (fromPos.Item2 - toPos.Item2 == STEP_SIZE)) {
                return Direction.NW;
            }
            return Direction.N;
        }
    }
}
