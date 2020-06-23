using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WhereAmI {
    public partial class Form1 : Form {

        const int WHITE = 16777215;
        const int RGB_R = 65536;
        const int RGB_G = 256;
        const int RGB_B = 1;

        BackgroundWorker searchWorker, renderWorker;
        Bitmap londonBitmap;
        int bitmapHeight, bitmapWidth;
        int[,] bitmapData;
        int[] countMap;
        object myLock = new object();
        bool renderingInProgress = false;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            //this.londonBitmap = new Bitmap(File.Exists("foo.png") ? Image.FromFile("foo.png") : Image.FromFile("Resources/london.png"));
            this.londonBitmap = new Bitmap(Image.FromFile("Resources/london.png"));
            //pictureBox1.Image = this.londonBitmap;
            this.bitmapHeight = this.londonBitmap.Height;
            this.bitmapWidth = this.londonBitmap.Width;
            this.bitmapData = new int[ this.bitmapWidth, this.bitmapHeight ];

            // temp for norman ^^
            this.countMap = new int[ WHITE + 1 ];
            for (int y = 0; y < this.bitmapHeight; y++) {
                for (int x = 0; x < this.bitmapWidth; x++) {
                    Color currentColor = this.londonBitmap.GetPixel(x, y);
                    this.countMap[ currentColor.R * RGB_R + currentColor.G * RGB_G + currentColor.B ]++;
                }
            }
            int r = 0, g = 0, b = 0;
            for (int i = 0; i < WHITE + 1; i++) {
                if (this.countMap[ i ] == 1) {
                    r = i / RGB_R;
                    g = (i - (r * RGB_R)) / RGB_G;
                    b = i - (r * RGB_R) - (g * RGB_G);
                    Console.WriteLine("Found single color: r" + r + " g" + g + " b" + b);
                }
            }
            for (int y = 0; y < this.bitmapHeight; y++) {
                for (int x = 0; x < this.bitmapWidth; x++) {
                    Color currentColor = this.londonBitmap.GetPixel(x, y);
                    if ((currentColor.R == r) && (currentColor.G == g) && (currentColor.B == b)) {
                        Console.WriteLine("Found at position: " + x + ":" + y + ".");
                    }
                }
            }
            return;

            // read image data into array
            for (int y = 0; y < this.bitmapHeight; y++) {
                for (int x = 0; x < this.bitmapWidth; x++) {
                    Color currentColor = this.londonBitmap.GetPixel(x, y);
                    this.bitmapData[ x, y ] = RGB_R * currentColor.R + RGB_G * currentColor.G + RGB_B * currentColor.B;
                }
            }

            searchWorker = new BackgroundWorker {
                WorkerReportsProgress = true
            };
            searchWorker.DoWork += Worker_DoWork;
            searchWorker.ProgressChanged += Worker_ProgressChanged;
            searchWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            searchWorker.RunWorkerAsync();

            renderWorker = new BackgroundWorker();
            renderWorker.DoWork += RenderWorker_DoWork;
            renderWorker.RunWorkerCompleted += RenderWorker_RunWorkerCompleted;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            // check every pixel
            for (int y1 = 0; y1 < this.bitmapHeight; y1++) {
                for (int x1 = 0; x1 < this.bitmapWidth; x1++) {
                    int currentColor = this.bitmapData[ x1, y1 ];
                    // that is not white
                    if (currentColor != WHITE) {
                        Console.WriteLine("Found non-white pixel at " + x1 + ":" + y1 + ".");
                        // if there is a second pixel
                        int instanceCount = 0;
                        for (int y2 = 0; y2 < this.bitmapHeight; y2++) {
                            for (int x2 = 0; x2 < this.bitmapWidth; x2++) {
                                // of the same color (not the same pixel again)
                                if (currentColor == this.bitmapData[ x2, y2 ]) {
                                    instanceCount++;
                                    this.bitmapData[ x2, y2 ] = WHITE;
                                }
                            }
                        }
                        // found the position
                        if (instanceCount == 1) {
                            worker.ReportProgress(100, new Tuple<int, int>(x1, y1));
                        }
                        else {
                            Console.WriteLine("Instance count: " + instanceCount);
                        }
                    }
                    worker.ReportProgress(0);
                }
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (e.ProgressPercentage == 100) {
                Tuple<int, int> bla = e.UserState as Tuple<int, int>;
                Console.WriteLine("Found something at " + bla.Item1 + ":" + bla.Item2 + ".");
                MessageBox.Show(this, "Found something at " + bla.Item1 + ":" + bla.Item2 + ".");
                return;
            }

            lock (this.myLock) {
                if (!this.renderingInProgress) {
                    this.renderingInProgress = true;
                    this.renderWorker.RunWorkerAsync();
                }
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show(this, "Done.");
        }

        private void RenderWorker_DoWork(object sender, DoWorkEventArgs e) {
            for (int y = 0; y < this.bitmapHeight; y++) {
                for (int x = 0; x < this.bitmapWidth; x++) {
                    int r = this.bitmapData[ x, y ] / RGB_R;
                    int g = (this.bitmapData[ x, y ] - (r * RGB_R)) / RGB_G;
                    int b = this.bitmapData[ x, y ] - (r * RGB_R) - (g * RGB_G);
                    londonBitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
        }

        private void RenderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.londonBitmap.Save("foo.png");
            //this.pictureBox1.Image = this.londonBitmap;
            this.renderingInProgress = false;
            Console.WriteLine("Image update.");
        }
    }
}
