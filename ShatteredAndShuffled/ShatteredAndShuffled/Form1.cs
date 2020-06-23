using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Linq;
using System.Drawing.Imaging;

namespace ShatteredAndShuffled {
    public partial class Form1 : Form {

        const int PUZZLE_WIDTH = 80;
        const int PUZZLE_HEIGHT = 60;
        const string DATA_FILE = "foo.xml";
        const string PIECE_DIRECTORY = "C:\\Users\\rkrausse\\Downloads\\pieces";

        private HashSet<PieceData> pieceData;
        private readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(HashSet<PieceData>));
        private readonly PieceData[,] solvedPuzzle = new PieceData[PUZZLE_WIDTH, PUZZLE_HEIGHT];

        // check all directions and see if there is a neighbor to be found; return all of them including their direction
        private List<Tuple<PieceData, PieceData.DIRECTION>> GetNeighbors(int x, int y) {
            List<Tuple<PieceData, PieceData.DIRECTION>> retVal = new List<Tuple<PieceData, PieceData.DIRECTION>>();
            if ((x > 0) && (this.solvedPuzzle[x - 1, y] != null)) {
                retVal.Add(new Tuple<PieceData, PieceData.DIRECTION>(this.solvedPuzzle[x - 1, y], PieceData.DIRECTION.WEST));
            }
            if ((x < PUZZLE_WIDTH - 1) && (this.solvedPuzzle[x + 1, y] != null)) {
                retVal.Add(new Tuple<PieceData, PieceData.DIRECTION>(this.solvedPuzzle[x + 1, y], PieceData.DIRECTION.EAST));
            }
            if ((y > 0) && (this.solvedPuzzle[x, y - 1] != null)) {
                retVal.Add(new Tuple<PieceData, PieceData.DIRECTION>(this.solvedPuzzle[x, y - 1], PieceData.DIRECTION.NORTH));
            }
            if ((y < PUZZLE_HEIGHT - 1) && (this.solvedPuzzle[x, y + 1] != null)) {
                retVal.Add(new Tuple<PieceData, PieceData.DIRECTION>(this.solvedPuzzle[x, y + 1], PieceData.DIRECTION.SOUTH));
            }
            return retVal;
        }

        // return all empty slots that have at least one neighbor and all the (unused) pieces that physically fit into that slot
        private List<Tuple<int, int, List<PieceData>>> GetNextPossibleStepsAndChoiceCount() {
            List<Tuple<int, int, List<PieceData>>> retVal = new List<Tuple<int, int, List<PieceData>>>();
            for (int y = 0; y < PUZZLE_HEIGHT; y++) {
                for (int x = 0; x < PUZZLE_WIDTH; x++) {
                    if (this.solvedPuzzle[x, y] == null) {
                        List<Tuple<PieceData, PieceData.DIRECTION>> neighbors = GetNeighbors(x, y);
                        if (neighbors.Count > 0) {
                            IEnumerable<PieceData> unusedPieces = this.pieceData.Where(piece => !piece.AlreadyUsed);
                            IEnumerable<PieceData> unusedPiecesMatchingN = unusedPieces.Where(piece => !neighbors.Any(nei => nei.Item2 == PieceData.DIRECTION.NORTH) || PieceData.EdgePatternMatches(this.solvedPuzzle[x, y - 1], piece, PieceData.DIRECTION.SOUTH));
                            IEnumerable<PieceData> unusedPiecesMatchingNS = unusedPiecesMatchingN.Where(piece => !neighbors.Any(nei => nei.Item2 == PieceData.DIRECTION.SOUTH) || PieceData.EdgePatternMatches(this.solvedPuzzle[x, y + 1], piece, PieceData.DIRECTION.NORTH));
                            IEnumerable<PieceData> unusedPiecesMatchingNSE = unusedPiecesMatchingNS.Where(piece => !neighbors.Any(nei => nei.Item2 == PieceData.DIRECTION.EAST) || PieceData.EdgePatternMatches(this.solvedPuzzle[x + 1, y], piece, PieceData.DIRECTION.WEST));
                            IEnumerable<PieceData> unusedPiecesMatchingNSEW = unusedPiecesMatchingNSE.Where(piece => !neighbors.Any(nei => nei.Item2 == PieceData.DIRECTION.WEST) || PieceData.EdgePatternMatches(this.solvedPuzzle[x - 1, y], piece, PieceData.DIRECTION.EAST));
                            retVal.Add(new Tuple<int, int, List<PieceData>>(x, y, unusedPiecesMatchingNSEW.ToList()));
                        }
                    }
                }
            }
            return retVal;
        }

        // return all (sorted) similarity measures for a slot and a list of given pieces
        private List<Tuple<PieceData, long>> GetSimilarityMeasures(int x, int y, List<PieceData> pieces) {
            List<Tuple<PieceData, PieceData.DIRECTION>> neighbors = GetNeighbors(x, y);
            // check every single direction (if neighbor is present)
            List<Tuple<PieceData, long>> guestimationsNorth = (y > 0) ? PieceData.GuestimateMatchingPiece(this.solvedPuzzle[x, y - 1], pieces, PieceData.DIRECTION.SOUTH) : new List<Tuple<PieceData, long>>();
            List<Tuple<PieceData, long>> guestimationsSouth = (y < PUZZLE_HEIGHT - 1) ? PieceData.GuestimateMatchingPiece(this.solvedPuzzle[x, y + 1], pieces, PieceData.DIRECTION.NORTH) : new List<Tuple<PieceData, long>>();
            List<Tuple<PieceData, long>> guestimationsEast = (x < PUZZLE_WIDTH - 1) ? PieceData.GuestimateMatchingPiece(this.solvedPuzzle[x + 1, y], pieces, PieceData.DIRECTION.WEST) : new List<Tuple<PieceData, long>>();
            List<Tuple<PieceData, long>> guestimationsWest = (x > 0) ? PieceData.GuestimateMatchingPiece(this.solvedPuzzle[x - 1, y], pieces, PieceData.DIRECTION.EAST) : new List<Tuple<PieceData, long>>();
            List<Tuple<PieceData, long>> guestimatedAverage = new List<Tuple<PieceData, long>>();
            foreach (PieceData piece in pieces) {
                int guestimateCount = 0;
                long guestimateSum = 0;
                if (guestimationsNorth.Count > 0) {
                    guestimateCount++;
                    guestimateSum += guestimationsNorth.FirstOrDefault(guess => guess.Item1.Equals(piece)).Item2;
                }
                if (guestimationsSouth.Count > 0) {
                    guestimateCount++;
                    guestimateSum += guestimationsSouth.FirstOrDefault(guess => guess.Item1.Equals(piece)).Item2;
                }
                if (guestimationsEast.Count > 0) {
                    guestimateCount++;
                    guestimateSum += guestimationsEast.FirstOrDefault(guess => guess.Item1.Equals(piece)).Item2;
                }
                if (guestimationsWest.Count > 0) {
                    guestimateCount++;
                    guestimateSum += guestimationsWest.FirstOrDefault(guess => guess.Item1.Equals(piece)).Item2;
                }
                guestimatedAverage.Add(new Tuple<PieceData, long>(piece, guestimateSum / guestimateCount));
            }
            guestimatedAverage.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            return guestimatedAverage;
        }

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            // try to find xml meta data file
            if (File.Exists(DATA_FILE)) {
                using (TextReader reader = new StreamReader(DATA_FILE)) {
                    this.pieceData = this.xmlSerializer.Deserialize(reader) as HashSet<PieceData>;
                }
            }
            // if it does not exist, create it
            if (this.pieceData == null) {
                this.pieceData = new HashSet<PieceData>();
                foreach (string fileLocation in Directory.GetFiles(PIECE_DIRECTORY)) {
                    this.pieceData.Add(new PieceData(fileLocation, new Bitmap(fileLocation)));
                }
                using (TextWriter writer = new StreamWriter(DATA_FILE)) {
                    this.xmlSerializer.Serialize(writer, this.pieceData);
                }
            }

            return;

            // put the corners in place
            this.solvedPuzzle[0, 0] = this.pieceData.FirstOrDefault(x => x.IsBorderNorth && x.IsBorderWest);
            this.solvedPuzzle[0, 0].AlreadyUsed = true;
            this.solvedPuzzle[PUZZLE_WIDTH - 1, 0] = this.pieceData.FirstOrDefault(x => x.IsBorderNorth && x.IsBorderEast);
            this.solvedPuzzle[PUZZLE_WIDTH - 1, 0].AlreadyUsed = true;
            this.solvedPuzzle[0, PUZZLE_HEIGHT - 1] = this.pieceData.FirstOrDefault(x => x.IsBorderSouth && x.IsBorderWest);
            this.solvedPuzzle[0, PUZZLE_HEIGHT - 1].AlreadyUsed = true;
            this.solvedPuzzle[PUZZLE_WIDTH - 1, PUZZLE_HEIGHT - 1] = this.pieceData.FirstOrDefault(x => x.IsBorderSouth && x.IsBorderEast);
            this.solvedPuzzle[PUZZLE_WIDTH - 1, PUZZLE_HEIGHT - 1].AlreadyUsed = true;
            // check that we found all corners
            if ((this.solvedPuzzle[0, 0] == null) || (this.solvedPuzzle[PUZZLE_WIDTH - 1, 0] == null) || (this.solvedPuzzle[0, PUZZLE_HEIGHT - 1] == null) || (this.solvedPuzzle[PUZZLE_WIDTH - 1, PUZZLE_HEIGHT - 1] == null)) {
                txtBoxLog.AppendText("Could not find all corners." + Environment.NewLine);
                return;
            }
            else {
                txtBoxLog.AppendText("Found all corners." + Environment.NewLine);
            }
            // try building northern border (-1 because of index, -1 for both corners)
            for (int borderX = 0; borderX <= PUZZLE_WIDTH - 3; borderX++) {
                IEnumerable<PieceData> northPieces = this.pieceData.Where(x => x.IsBorderNorth);
                IEnumerable<PieceData> northAndUnusedPieces = northPieces.Where(x => !x.AlreadyUsed);
                IEnumerable<PieceData> northAndUnusedAndMatchingPieces = northAndUnusedPieces.Where(x => PieceData.EdgePatternMatches(this.solvedPuzzle[borderX, 0], x, PieceData.DIRECTION.EAST));
                Console.WriteLine("North:" + northPieces.Count() + " NorthUnused:" + northAndUnusedPieces.Count() + " NorthUnusedMatching:" + northAndUnusedAndMatchingPieces.Count());
                if (northAndUnusedAndMatchingPieces.Count() == 0) {
                    txtBoxLog.AppendText("Could not finish northern border at position #" + borderX + "." + Environment.NewLine);
                    break;
                }
                List<Tuple<PieceData, long>> guestimatedSimilarities = PieceData.GuestimateMatchingPiece(this.solvedPuzzle[borderX, 0], northAndUnusedAndMatchingPieces.ToList(), PieceData.DIRECTION.EAST);
                if (guestimatedSimilarities.Count > 0) {
                    guestimatedSimilarities.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                    this.solvedPuzzle[borderX + 1, 0] = guestimatedSimilarities[0].Item1;
                    this.solvedPuzzle[borderX + 1, 0].AlreadyUsed = true;
                    Console.WriteLine("Found piece for " + (borderX + 1) + ":0");
                }
                else {
                    txtBoxLog.AppendText("Could NOT find a matching piece for " + (borderX + 1) + ":0" + Environment.NewLine);
                }
            }
            // try building southern border (-1 because of index, -1 for both corners)
            for (int borderX = 0; borderX <= PUZZLE_WIDTH - 3; borderX++) {
                IEnumerable<PieceData> southPieces = this.pieceData.Where(x => x.IsBorderSouth);
                IEnumerable<PieceData> southAndUnusedPieces = southPieces.Where(x => !x.AlreadyUsed);
                IEnumerable<PieceData> southAndUnusedAndMatchingPieces = southAndUnusedPieces.Where(x => PieceData.EdgePatternMatches(this.solvedPuzzle[borderX, PUZZLE_HEIGHT - 1], x, PieceData.DIRECTION.EAST));
                Console.WriteLine("South:" + southPieces.Count() + " SouthUnused:" + southAndUnusedPieces.Count() + " SouthUnusedMatching:" + southAndUnusedAndMatchingPieces.Count());
                if (southAndUnusedAndMatchingPieces.Count() == 0) {
                    txtBoxLog.AppendText("Could not finish southern border at position #" + borderX + "." + Environment.NewLine);
                    break;
                }
                List<Tuple<PieceData, long>> guestimatedSimilarities = PieceData.GuestimateMatchingPiece(this.solvedPuzzle[borderX, PUZZLE_HEIGHT - 1], southAndUnusedAndMatchingPieces.ToList(), PieceData.DIRECTION.EAST);
                if (guestimatedSimilarities.Count > 0) {
                    guestimatedSimilarities.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                    this.solvedPuzzle[borderX + 1, PUZZLE_HEIGHT - 1] = guestimatedSimilarities[0].Item1;
                    this.solvedPuzzle[borderX + 1, PUZZLE_HEIGHT - 1].AlreadyUsed = true;
                    Console.WriteLine("Found piece for " + (borderX + 1) + ":" + (PUZZLE_HEIGHT - 1));
                }
                else {
                    txtBoxLog.AppendText("Could NOT find a matching piece for " + (borderX + 1) + ":" + (PUZZLE_HEIGHT - 1) + Environment.NewLine);
                }
            }
            // try building eastern border (-1 because of index, -1 for both corners)
            for (int borderY = 0; borderY <= PUZZLE_HEIGHT - 3; borderY++) {
                IEnumerable<PieceData> easternPieces = this.pieceData.Where(x => x.IsBorderEast);
                IEnumerable<PieceData> easternAndUnusedPieces = easternPieces.Where(x => !x.AlreadyUsed);
                IEnumerable<PieceData> easternAndUnusedAndMatchingPieces = easternAndUnusedPieces.Where(x => PieceData.EdgePatternMatches(this.solvedPuzzle[PUZZLE_WIDTH - 1, borderY], x, PieceData.DIRECTION.SOUTH));
                Console.WriteLine("Eastern:" + easternPieces.Count() + " EasternUnused:" + easternAndUnusedPieces.Count() + " EasternUnusedMatching:" + easternAndUnusedAndMatchingPieces.Count());
                if (easternAndUnusedAndMatchingPieces.Count() == 0) {
                    txtBoxLog.AppendText("Could not finish eastern border at position #" + borderY + "." + Environment.NewLine);
                    break;
                }
                List<Tuple<PieceData, long>> guestimatedSimilarities = PieceData.GuestimateMatchingPiece(this.solvedPuzzle[PUZZLE_WIDTH - 1, borderY], easternAndUnusedAndMatchingPieces.ToList(), PieceData.DIRECTION.SOUTH);
                if (guestimatedSimilarities.Count > 0) {
                    guestimatedSimilarities.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                    this.solvedPuzzle[PUZZLE_WIDTH - 1, borderY + 1] = guestimatedSimilarities[0].Item1;
                    this.solvedPuzzle[PUZZLE_WIDTH - 1, borderY + 1].AlreadyUsed = true;
                    Console.WriteLine("Found piece for " + (PUZZLE_WIDTH - 1) + ":" + (borderY + 1));
                }
                else {
                    txtBoxLog.AppendText("Could NOT find a matching piece for " + (PUZZLE_WIDTH - 1) + ":" + (borderY + 1) + Environment.NewLine);
                }
            }

            // test
            List<Tuple<int, int, List<PieceData>>> nextPossibleSteps = GetNextPossibleStepsAndChoiceCount();
            while (nextPossibleSteps.Count > 0) {
                Console.WriteLine("Pieces left: " + this.pieceData.Count(x => !x.AlreadyUsed));
                if (nextPossibleSteps.Any(x => x.Item3.Count == 0)) {
                    txtBoxLog.AppendText("fucked up" + Environment.NewLine);
                    break;
                }
                if (nextPossibleSteps.Any(x => x.Item3.Count == 1)) {
                    Tuple<int, int, List<PieceData>> uniqueSolution = nextPossibleSteps.FirstOrDefault(x => x.Item3.Count == 1);
                    this.solvedPuzzle[uniqueSolution.Item1, uniqueSolution.Item2] = uniqueSolution.Item3[0];
                    this.solvedPuzzle[uniqueSolution.Item1, uniqueSolution.Item2].AlreadyUsed = true;
                    txtBoxLog.AppendText("Found a unique piece! :-D" + Environment.NewLine);
                    nextPossibleSteps = GetNextPossibleStepsAndChoiceCount();
                    continue;
                }
                txtBoxLog.AppendText("Found " + nextPossibleSteps.Count() + " next possible steps." + Environment.NewLine);
                nextPossibleSteps.Sort((x, y) => x.Item3.Count.CompareTo(y.Item3.Count));
                txtBoxLog.AppendText("In order of choice count: " + string.Join(" , ", nextPossibleSteps.Select(x => x.Item1 + ":" + x.Item2 + "#" + x.Item3.Count)) + Environment.NewLine);

                // measure all promising possibilities (<10 choices)
                List<Tuple<int, int, List<Tuple<PieceData, long>>>> measuredPossibilities = new List<Tuple<int, int, List<Tuple<PieceData, long>>>>();
                foreach (Tuple<int, int, List<PieceData>> possibility in nextPossibleSteps.Where(x => x.Item3.Count < 10)) {
                    txtBoxLog.AppendText(possibility.Item1 + ":" + possibility.Item2 + Environment.NewLine);
                    measuredPossibilities.Add(new Tuple<int, int, List<Tuple<PieceData, long>>>(possibility.Item1, possibility.Item2, GetSimilarityMeasures(possibility.Item1, possibility.Item2, possibility.Item3)));
                    foreach (Tuple<PieceData, long> item in measuredPossibilities.Last().Item3) {
                        txtBoxLog.AppendText(item.Item1.Filename + " ~" + item.Item2 + Environment.NewLine);
                    }
                }

                txtBoxLog.AppendText("Sorted:" + Environment.NewLine);
                // use the possibility with the biggest relative difference between measurement #1 and #2
                measuredPossibilities.Sort((x, y) => ((double)y.Item3[1].Item2 / y.Item3[0].Item2).CompareTo((double)x.Item3[1].Item2 / x.Item3[0].Item2));
                foreach (Tuple<int, int, List<Tuple<PieceData, long>>> item in measuredPossibilities) {
                    txtBoxLog.AppendText(item.Item1 + ":" + item.Item2 + Environment.NewLine);
                }
                this.solvedPuzzle[measuredPossibilities[0].Item1, measuredPossibilities[0].Item2] = measuredPossibilities[0].Item3[0].Item1;
                this.solvedPuzzle[measuredPossibilities[0].Item1, measuredPossibilities[0].Item2].AlreadyUsed = true;

                nextPossibleSteps = GetNextPossibleStepsAndChoiceCount();
            }

            //// build image row by row (there should be less conflicts due to the two edges that have to match up)
            //bool problem = false;
            //for (int pieceY = 1; pieceY < PUZZLE_HEIGHT; pieceY++) {
            //    for (int pieceX = PUZZLE_WIDTH - 2; pieceX >= 0; pieceX--) {
            //        IEnumerable<PieceData> unusedPieces = this.pieceData.Where(x => !x.AlreadyUsed);
            //        IEnumerable<PieceData> unusedPiecesMatchingNorth = unusedPieces.Where(x => PieceData.EdgePatternMatches(this.solvedPuzzle[ pieceX, pieceY - 1 ], x, PieceData.DIRECTION.SOUTH));
            //        IEnumerable<PieceData> unusedPiecesMatchingNorthAndEast = unusedPiecesMatchingNorth.Where(x => PieceData.EdgePatternMatches(this.solvedPuzzle[ pieceX + 1, pieceY ], x, PieceData.DIRECTION.WEST));
            //        Console.WriteLine("Unused:" + unusedPieces.Count() + " UnusedMatchingNorth:" + unusedPiecesMatchingNorth.Count() + " UnusedMatchingNorthAndEast:" + unusedPiecesMatchingNorthAndEast.Count());
            //        if (unusedPiecesMatchingNorthAndEast.Count() == 0) {
            //            Console.WriteLine("Could not finish row #" + pieceY + " at position #" + pieceX + ".");
            //            problem = true;
            //            break;
            //        }

            //        List<Tuple<PieceData, long>> guestimatedSimilaritiesWest = PieceData.GuestimateMatchingPiece(this.solvedPuzzle[ pieceX + 1, pieceY ], unusedPiecesMatchingNorthAndEast.ToList(), PieceData.DIRECTION.WEST);
            //        List<Tuple<PieceData, long>> guestimatedSimilaritiesSouth = PieceData.GuestimateMatchingPiece(this.solvedPuzzle[ pieceX, pieceY - 1 ], unusedPiecesMatchingNorthAndEast.ToList(), PieceData.DIRECTION.SOUTH);
            //        List<Tuple<PieceData, long>> guestimatedAverage = guestimatedSimilaritiesWest.Select(x => new Tuple<PieceData, long>(x.Item1, (x.Item2 + guestimatedSimilaritiesSouth.FirstOrDefault(y => y.Item1.Filename.Equals(x.Item1.Filename)).Item2) / 2)).ToList();
            //        if (guestimatedAverage.Count > 0) {
            //            guestimatedAverage.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            //            this.solvedPuzzle[ pieceX, pieceY ] = guestimatedAverage[ 0 ].Item1;
            //            this.solvedPuzzle[ pieceX, pieceY ].AlreadyUsed = true;
            //            Console.WriteLine("Found piece for " + pieceX + ":" + pieceY);
            //        }
            //        else {
            //            txtBoxLog.AppendText("Could NOT find a matching piece for " + pieceX + ":" + pieceY + Environment.NewLine);
            //        }
            //    }
            //    if (problem) {
            //        break;
            //    }
            //}

            // draw image and show
            Bitmap output = new Bitmap(2 + PUZZLE_WIDTH * (PieceData.DATA_WIDTH - 4) + 2, 2 + PUZZLE_HEIGHT * (PieceData.DATA_HEIGHT - 4) + 2);
            for (int solvedY = 0; solvedY < PUZZLE_HEIGHT; solvedY++) {
                for (int solvedX = 0; solvedX < PUZZLE_WIDTH; solvedX++) {
                    if (this.solvedPuzzle[solvedX, solvedY] == null) {
                        continue;
                    }
                    for (int dataY = 0; dataY < PieceData.DATA_HEIGHT; dataY++) {
                        for (int dataX = 0; dataX < PieceData.DATA_WIDTH; dataX++) {
                            PieceData.ColorTupleRGB foo = this.solvedPuzzle[solvedX, solvedY].GetImagePixel(dataX, dataY);
                            if ((foo.R != 255) && (foo.G != 255) && (foo.B != 255)) {
                                output.SetPixel((solvedX * (PieceData.DATA_WIDTH - 4) + 2) + (dataX - 2), (solvedY * (PieceData.DATA_HEIGHT - 4) + 2) + (dataY - 2), Color.FromArgb(foo.R, foo.G, foo.B));
                            }
                        }
                    }
                }
            }
            pictureBox1.Image = output;
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK) {
                pictureBox1.Image.Save(saveFileDialog1.FileName, ImageFormat.Png);
            }
        }
    }
}
