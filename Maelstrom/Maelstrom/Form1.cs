using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Maelstrom {
    public partial class Form1 : Form {

        enum DIRECTION {
            E = 0, S = 1, W = 2, N = 3
        }

        const int CHAR_SLOT_WIDTH = 5;
        const int CHAR_SLOT_HEIGHT = 5;
        const int CHAR_WIDTH = 3;
        const int CHAR_HEIGHT = 5;
        static readonly byte[][,] KNOWN_CHARACTERS = {
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 1, 0, 1 }, { 1, 0, 1 }, { 1, 0, 1 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 0, 1, 0 }, { 1, 1, 0 }, { 0, 1, 0 }, { 0, 1, 0 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 0, 0, 1 }, { 1, 1, 1 }, { 1, 0, 0 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 0, 0, 1 }, { 0, 1, 1 }, { 0, 0, 1 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 0, 0 }, { 1, 0, 1 }, { 1, 1, 1 }, { 0, 0, 1 }, { 0, 0, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 1, 0, 0 }, { 1, 1, 1 }, { 0, 0, 1 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 1, 0, 0 }, { 1, 1, 1 }, { 1, 0, 1 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 0, 0, 1 }, { 0, 0, 1 }, { 0, 0, 1 }, { 0, 0, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 1, 0, 1 }, { 1, 1, 1 }, { 1, 0, 1 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 1, 0, 1 }, { 1, 1, 1 }, { 0, 0, 1 }, { 0, 0, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 1, 0, 1 }, { 1, 1, 1 }, { 1, 0, 1 }, { 1, 0, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 1, 1 }, { 1, 0, 1 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 1, 0, 0 }, { 1, 0, 0 }, { 1, 0, 0 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 0 }, { 1, 0, 1 }, { 1, 0, 1 }, { 1, 0, 1 }, { 1, 1, 0 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 1, 0, 0 }, { 1, 1, 0 }, { 1, 0, 0 }, { 1, 1, 1 } },
            new byte[CHAR_HEIGHT, CHAR_WIDTH] { { 1, 1, 1 }, { 1, 0, 0 }, { 1, 1, 0 }, { 1, 0, 0 }, { 1, 0, 0 } }
        };

        private BackgroundWorker bgWorker;
        private Bitmap maelstromBitmap;
        private Graphics maelstromGraphics;
        private string hexString;

        public Form1() {
            InitializeComponent();
        }

        private static DIRECTION TakeTurn(DIRECTION currentDirection) {
            return (DIRECTION) ((int) (currentDirection + 1) % 4);
        }

        private static byte[] StringToByteArray(string hex) {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[ NumberChars / 2 ];
            for (int i = 0; i < NumberChars; i += 2) {
                bytes[ i / 2 ] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private static byte[,] GetCharacterBits(Bitmap bitmap, int x, int y, DIRECTION currentDirection) {
            byte[,] result = new byte[ CHAR_SLOT_WIDTH, CHAR_SLOT_HEIGHT ];
            switch (currentDirection) {
                case DIRECTION.E:
                    for (int i = 0; i < CHAR_SLOT_WIDTH; i++) {
                        for (int j = 0; j < CHAR_SLOT_HEIGHT; j++) {
                            result[ i, j ] = (bitmap.GetPixel(x + i, y + j).GetBrightness() < 0.95) ? (byte) 1 : (byte) 0;
                        }
                    }
                    break;
                case DIRECTION.S:
                    for (int i = 0; i < CHAR_SLOT_WIDTH; i++) {
                        for (int j = CHAR_SLOT_HEIGHT - 1; j >= 0; j--) {
                            result[ i, CHAR_SLOT_HEIGHT - j - 1 ] = (bitmap.GetPixel(x + j, y + i).GetBrightness() < 0.95) ? (byte) 1 : (byte) 0;
                        }
                    }
                    break;
                case DIRECTION.W:
                    for (int i = CHAR_SLOT_WIDTH - 1; i >= 0; i--) {
                        for (int j = CHAR_SLOT_HEIGHT - 1; j >= 0; j--) {
                            result[ CHAR_SLOT_WIDTH - i - 1, CHAR_SLOT_HEIGHT - j - 1 ] = (bitmap.GetPixel(x + i, y + j).GetBrightness() < 0.95) ? (byte) 1 : (byte) 0;
                        }
                    }
                    break;
                case DIRECTION.N:
                    for (int i = 0; i < CHAR_SLOT_WIDTH; i++) {
                        for (int j = CHAR_SLOT_HEIGHT - 1; j >= 0; j--) {
                            result[ CHAR_SLOT_HEIGHT - j - 1, i ] = (bitmap.GetPixel(x + i, y + j).GetBrightness() < 0.95) ? (byte) 1 : (byte) 0;
                        }
                    }
                    break;
            }
            return result;
        }

        private static byte DetermineCharacter(byte[,] bits) {
            for (byte charIndex = 0; charIndex < KNOWN_CHARACTERS.Length; charIndex++) {
                for (byte offset = 0; offset < CHAR_SLOT_WIDTH - CHAR_WIDTH + 1; offset++) {
                    bool found = true;
                    for (byte y = 0; y < CHAR_HEIGHT; y++) {
                        for (byte x = 0; x < CHAR_WIDTH; x++) {
                            found = found && (bits[ x + offset, y ] == KNOWN_CHARACTERS[ charIndex ][ y, x ]);
                        }
                    }
                    if (found) {
                        return charIndex;
                    }
                }
            }
            Console.WriteLine("Error determining character.");
            return 0;
        }

        private static Tuple<int, int> TakeStep(int x, int y, DIRECTION direction) {
            switch (direction) {
                case DIRECTION.E:
                    return new Tuple<int, int>(x + CHAR_SLOT_WIDTH, y);
                case DIRECTION.S:
                    return new Tuple<int, int>(x, y + CHAR_SLOT_HEIGHT);
                case DIRECTION.W:
                    return new Tuple<int, int>(x - CHAR_SLOT_WIDTH, y);
                case DIRECTION.N:
                    return new Tuple<int, int>(x, y - CHAR_SLOT_HEIGHT);
                default:
                    return new Tuple<int, int>(-1, -1);
            }
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
            // access image
            this.maelstromBitmap = new Bitmap(pictureBox1.Image);
            this.maelstromGraphics = Graphics.FromImage(pictureBox1.Image);

            // start bg worker
            this.bgWorker = new BackgroundWorker();
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.DoWork += BgWorker_DoWork;
            this.bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            this.bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            this.bgWorker.RunWorkerAsync();
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e) {
            int minX = 0;
            int maxX = this.maelstromBitmap.Width - 1;
            int minY = 0;
            int maxY = this.maelstromBitmap.Height - 1;
            int currentX = 0;
            int currentY = 0;
            Tuple<int, int> nextPosition;
            DIRECTION currentDirection = DIRECTION.E;
            List<byte> result = new List<byte>();
            bool done = false;

            while (!done) {
                result.Add(DetermineCharacter(GetCharacterBits(this.maelstromBitmap, currentX, currentY, currentDirection)));
                nextPosition = TakeStep(currentX, currentY, currentDirection);
                bool hitWall = false;
                switch (currentDirection) {
                    case DIRECTION.E:
                        if (nextPosition.Item1 > maxX) {
                            hitWall = true;
                            minY += CHAR_SLOT_HEIGHT;
                        }
                        break;
                    case DIRECTION.S:
                        if (nextPosition.Item2 > maxY) {
                            hitWall = true;
                            maxX -= CHAR_SLOT_WIDTH;
                        }
                        break;
                    case DIRECTION.W:
                        if (nextPosition.Item1 < minX) {
                            hitWall = true;
                            maxY -= CHAR_SLOT_HEIGHT;
                        }
                        break;
                    case DIRECTION.N:
                        if (nextPosition.Item2 < minY) {
                            hitWall = true;
                            minX += CHAR_SLOT_WIDTH;
                        }
                        break;
                }
                if (hitWall) {
                    currentDirection = TakeTurn(currentDirection);
                    nextPosition = TakeStep(currentX, currentY, currentDirection);
                    if ((nextPosition.Item1 > maxX) || (nextPosition.Item2 > maxY) || (nextPosition.Item1 < minX) || (nextPosition.Item2 < minY)) {
                        done = true;
                    }
                }
                currentX = nextPosition.Item1;
                currentY = nextPosition.Item2;
            }

            e.Result = result;
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            Console.WriteLine("Progress.");
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            List<byte> halfByteList = e.Result as List<byte>;
            List<byte> byteList = new List<byte>();
            if (halfByteList != null) {
                for (int i = 0; i < halfByteList.Count; i++) {
                    byteList.Add((byte) ((byte) (halfByteList[ i ] << 4) | halfByteList[ ++i ]));
                }
            }
            File.WriteAllBytes("river.png", byteList.ToArray());

            Bitmap riverBitmap = new Bitmap("river.png");

            int xMin = 0;
            int xMax = riverBitmap.Width - 1;
            int yMin = 0;
            int yMax = riverBitmap.Height - 1;

            int currentX = 0;
            int currentY = 0;

            BitArray bitStream = new BitArray(riverBitmap.Width * riverBitmap.Height);
            int bitPosition = 0;

            while ((xMin < xMax) && (yMin < yMax)) {
                while (currentX <= xMax) {
                    bitStream[ bitPosition++ ] = riverBitmap.GetPixel(currentX++, currentY).GetBrightness() < 0.8f;
                }
                // already went over the limit .. take one step back
                currentX--;
                // need to take a step into the next direction
                currentY++;
                // reduce limit
                yMin++;
                while (currentY <= yMax) {
                    bitStream[ bitPosition++ ] = riverBitmap.GetPixel(currentX, currentY++).GetBrightness() < 0.8f;
                }
                // already went over the limit .. take one step back
                currentY--;
                // need to take a step into the next direction
                currentX--;
                // reduce limit
                xMax--;
                while (currentX >= xMin) {
                    bitStream[ bitPosition++ ] = riverBitmap.GetPixel(currentX--, currentY).GetBrightness() < 0.8f;
                }
                // already went over the limit .. take one step back
                currentX++;
                // need to take a step into the next direction
                currentY--;
                // reduce limit
                yMax--;
                while (currentY >= yMin) {
                    bitStream[ bitPosition++ ] = riverBitmap.GetPixel(currentX, currentY--).GetBrightness() < 0.8f;
                }
                // already went over the limit .. take one step back
                currentY++;
                // need to take a step into the next direction
                currentX++;
                // reduce limit
                xMin++;
            }

            Console.WriteLine(Encoding.ASCII.GetString(ToByteArray(bitStream)));

            int whitePixelCount = 0;
            for (int y = 0; y < this.maelstromBitmap.Height; y++) {
                for (int x = 0; x < this.maelstromBitmap.Width; x++) {
                    Color pixelColor = this.maelstromBitmap.GetPixel(x, y);
                    whitePixelCount += (pixelColor.R + pixelColor.G + pixelColor.B == 765) ? 1 : 0;
                }
            }
            Console.WriteLine("White pixel count: " + whitePixelCount);
        }
    }
}
