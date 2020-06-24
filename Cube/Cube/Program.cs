using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Cube {
    class Program {

        public const string SOLUTION_FILE = "solution.txt";
        public const byte CUBE_X = 4;
        public const byte CUBE_Y = 4;
        public const byte CUBE_Z = 4;

        public class KnownPiece {
            public byte PieceId { get; private set; }
            public byte DimX { get; private set; }
            public byte DimY { get; private set; }
            public byte DimZ { get; private set; }
            public byte[,,] Blocks { get; private set; }
            private string ByteFootprint {
                get {
                    string retVal = string.Empty;
                    for (int z = 0; z < this.DimZ; z++) {
                        for (int y = 0; y < this.DimY; y++) {
                            for (int x = 0; x < this.DimX; x++) {
                                retVal += this.Blocks[x, y, z];
                            }
                        }
                    }
                    return retVal;
                }
            }
            public string Footprint {
                get {
                    return this.DimX.ToString() + ":" + this.DimY.ToString() + ":" + this.DimZ.ToString() + "-" + this.ByteFootprint;
                }
            }

            public KnownPiece(byte id, byte x, byte y, byte z) {
                this.PieceId = id;
                this.DimX = x;
                this.DimY = y;
                this.DimZ = z;
                this.Blocks = new byte[x, y, z];
            }

            public KnownPiece(byte id, byte x, byte y, byte z, byte[] blocksXYZ) {
                this.PieceId = id;
                this.DimX = x;
                this.DimY = y;
                this.DimZ = z;
                this.Blocks = new byte[x, y, z];
                for (int c = 0; c < z; c++) {
                    for (int b = 0; b < y; b++) {
                        for (int a = 0; a < x; a++) {
                            this.Blocks[a, b, c] = blocksXYZ[x * y * c + x * b + a];
                        }
                    }
                }
            }

            override public string ToString() {
                return this.Footprint;
            }
        }

        private static readonly KnownPiece[] KNOWN_PIECES = {
            new KnownPiece(5, 3, 4, 2, new byte[] { 0, 0, 0, 5, 0, 0, 5, 0, 0, 5, 0, 0, 5, 5, 5, 5, 5, 0, 5, 0, 0, 5, 0, 0 }),
            new KnownPiece(6, 2, 4, 3, new byte[] { 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 6, 0, 6, 0, 6, 0, 6, 0, 6, 0, 0 }),
            new KnownPiece(3, 2, 3, 3, new byte[] { 3, 0, 0, 0, 0, 0, 3, 0, 3, 0, 0, 0, 3, 0, 3, 3, 3, 3 }),
            new KnownPiece(4, 2, 3, 3, new byte[] { 4, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 4, 4, 4, 4, 4, 0 }),
            new KnownPiece(9, 2, 4, 2, new byte[] { 0, 0, 0, 0, 9, 9, 0, 9, 9, 0, 9, 0, 9, 0, 9, 0 }),
            new KnownPiece(1, 3, 4, 1, new byte[] { 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 1, 0 }),
            new KnownPiece(2, 2, 4, 1, new byte[] { 0, 2, 0, 2, 0, 2, 2, 2 }),
            new KnownPiece(7, 2, 2, 2, new byte[] { 0, 7, 7, 7, 7, 7, 7, 7 }),
            new KnownPiece(8, 2, 4, 1, new byte[] { 0, 8, 8, 8, 8, 8, 0, 8 })
        };

        //// trial 3x3 puzzle
        //private static readonly KnownPiece[] KNOWN_PIECES = {
        //    new KnownPiece(1, 3, 3, 3, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1 }),
        //    new KnownPiece(2, 2, 2, 2, new byte[] { 2, 2, 2, 0, 2, 0, 0, 0 }),
        //    new KnownPiece(3, 3, 3, 3, new byte[] { 3, 3, 3, 3, 3, 3, 3, 3, 0, 3, 3, 3, 3, 0, 0, 3, 0, 0, 3, 3, 0, 3, 0, 0, 0, 0, 0 })
        //};

        //// another 3x3 trial
        //private static readonly KnownPiece[] KNOWN_PIECES = {
        //    new KnownPiece(1, 2, 3, 1, new byte[] { 1, 1, 0, 1, 0, 1 }),
        //    new KnownPiece(2, 2, 3, 1, new byte[] { 2, 2, 0, 2, 0, 2 }),
        //    new KnownPiece(3, 2, 3, 1, new byte[] { 3, 3, 0, 3, 0, 3 }),
        //    new KnownPiece(4, 2, 3, 1, new byte[] { 4, 4, 0, 4, 0, 4 }),
        //    new KnownPiece(5, 2, 2, 1, new byte[] { 5, 5, 0, 5 }),
        //    new KnownPiece(6, 2, 3, 1, new byte[] { 0, 6, 6, 6, 6, 0}),
        //    new KnownPiece(7, 2, 3, 1, new byte[] { 0, 7, 7, 7, 0, 7 })
        //};

        //// another 3x3 trial
        //private static readonly KnownPiece[] KNOWN_PIECES = {
        //    new KnownPiece(1, 2, 2, 2, new byte[] { 0, 0, 1, 0, 1, 0, 1, 1 }),
        //    new KnownPiece(2, 2, 2, 2, new byte[] { 0, 2, 2, 2, 0, 0, 2, 0 }),
        //    new KnownPiece(3, 2, 3, 1, new byte[] { 0, 3, 3, 3, 3, 0 }),
        //    new KnownPiece(4, 2, 2, 2, new byte[] { 0, 0, 4, 0, 0, 4, 4, 4 }),
        //    new KnownPiece(5, 2, 2, 1, new byte[] { 0, 5, 5, 5 }),
        //    new KnownPiece(6, 3, 2, 1, new byte[] { 0, 6, 0, 6, 6, 7 }),
        //    new KnownPiece(7, 3, 2, 1, new byte[] { 0, 0, 7, 7, 7, 7 })
        //};

        //// trivial 3x3 example
        //private static readonly KnownPiece[] KNOWN_PIECES = {
        //    new KnownPiece(1, 3, 1, 1, new byte[] { 1, 1, 1 }),
        //    new KnownPiece(2, 3, 1, 1, new byte[] { 1, 1, 1 }),
        //    new KnownPiece(3, 3, 1, 1, new byte[] { 1, 1, 1 }),
        //    new KnownPiece(4, 3, 1, 1, new byte[] { 1, 1, 1 }),
        //    new KnownPiece(5, 3, 1, 1, new byte[] { 1, 1, 1 }),
        //    new KnownPiece(6, 3, 1, 1, new byte[] { 1, 1, 1 }),
        //    new KnownPiece(7, 3, 1, 1, new byte[] { 1, 1, 1 }),
        //    new KnownPiece(8, 3, 1, 1, new byte[] { 1, 1, 1 }),
        //    new KnownPiece(9, 3, 1, 1, new byte[] { 1, 1, 1 })
        //};

        /// <summary>
        /// For all of the 6 sides (imagine a cube) we rotate the piece through the 4 orientation.
        /// This gives us, at most, 24 different ways of looking at the piece. Duplicates are eliminated.
        /// </summary>
        private static List<KnownPiece> GeneratePiecePermutations(KnownPiece piece) {
            List<KnownPiece> retVal = new List<KnownPiece>();
            // original #1 [front]
            KnownPiece zRotate1 = new KnownPiece(piece.PieceId, piece.DimX, piece.DimY, piece.DimZ);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        zRotate1.Blocks[x, y, z] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(zRotate1);
            // original #1 + 180 y
            KnownPiece zRotate2 = new KnownPiece(piece.PieceId, piece.DimX, piece.DimY, piece.DimZ);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        zRotate2.Blocks[piece.DimX - 1 - x, y, piece.DimZ - 1 - z] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(zRotate2);
            // original #1 + 180 x
            KnownPiece zRotate3 = new KnownPiece(piece.PieceId, piece.DimX, piece.DimY, piece.DimZ);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        zRotate3.Blocks[x, piece.DimY - 1 - y, piece.DimZ - 1 - z] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(zRotate3);
            // original #1 + 180 z
            KnownPiece zRotate4 = new KnownPiece(piece.PieceId, piece.DimX, piece.DimY, piece.DimZ);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        zRotate4.Blocks[piece.DimX - 1 - x, piece.DimY - 1 - y, z] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(zRotate4);
            // original #2 [front turned right]
            KnownPiece zRotate5 = new KnownPiece(piece.PieceId, piece.DimY, piece.DimX, piece.DimZ);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        zRotate5.Blocks[piece.DimY - 1 - y, x, z] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(zRotate5);
            // original #2 + 180 z
            KnownPiece zRotate6 = new KnownPiece(piece.PieceId, piece.DimY, piece.DimX, piece.DimZ);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        zRotate6.Blocks[y, piece.DimX - 1 - x, z] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(zRotate6);
            // original #2 + 180 y
            KnownPiece zRotate7 = new KnownPiece(piece.PieceId, piece.DimY, piece.DimX, piece.DimZ);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        zRotate7.Blocks[y, x, piece.DimZ - 1 - z] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(zRotate7);
            // original #2 + 180 x
            KnownPiece zRotate8 = new KnownPiece(piece.PieceId, piece.DimY, piece.DimX, piece.DimZ);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        zRotate8.Blocks[piece.DimY - 1 - y, piece.DimX - 1 - x, piece.DimZ - 1 - z] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(zRotate8);
            // original #3 [left]
            KnownPiece xRotate1 = new KnownPiece(piece.PieceId, piece.DimZ, piece.DimY, piece.DimX);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        xRotate1.Blocks[piece.DimZ - 1 - z, y, x] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(xRotate1);
            // original #3 +180 y
            KnownPiece xRotate2 = new KnownPiece(piece.PieceId, piece.DimZ, piece.DimY, piece.DimX);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        xRotate2.Blocks[z, y, piece.DimX - 1 - x] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(xRotate2);
            // original #3 + 180 z
            KnownPiece xRotate3 = new KnownPiece(piece.PieceId, piece.DimZ, piece.DimY, piece.DimX);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        xRotate3.Blocks[z, piece.DimY - 1 - y, x] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(xRotate3);
            // original #3 + 180 x
            KnownPiece xRotate4 = new KnownPiece(piece.PieceId, piece.DimZ, piece.DimY, piece.DimX);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        xRotate4.Blocks[piece.DimZ - 1 - z, piece.DimY - 1 - y, piece.DimX - 1 - x] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(xRotate4);
            // original #4 [left turned left]
            KnownPiece xRotate5 = new KnownPiece(piece.PieceId, piece.DimY, piece.DimZ, piece.DimX);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        xRotate5.Blocks[y, z, x] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(xRotate5);
            // original #4 + 180 x
            KnownPiece xRotate6 = new KnownPiece(piece.PieceId, piece.DimY, piece.DimZ, piece.DimX);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        xRotate6.Blocks[y, piece.DimZ - 1 - z, piece.DimX - 1 - x] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(xRotate6);
            // original #4 + 180 y
            KnownPiece xRotate7 = new KnownPiece(piece.PieceId, piece.DimY, piece.DimZ, piece.DimX);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        xRotate7.Blocks[piece.DimY - 1 - y, z, piece.DimX - 1 - x] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(xRotate7);
            // original #4 + 180 z
            KnownPiece xRotate8 = new KnownPiece(piece.PieceId, piece.DimY, piece.DimZ, piece.DimX);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        xRotate8.Blocks[piece.DimY - 1 - y, piece.DimZ - 1 - z, x] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(xRotate8);
            // original #5 [bottom]
            KnownPiece yRotate1 = new KnownPiece(piece.PieceId, piece.DimX, piece.DimZ, piece.DimY);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        yRotate1.Blocks[x, z, piece.DimY - 1 - y] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(yRotate1);
            // original #5 + 180 x
            KnownPiece yRotate2 = new KnownPiece(piece.PieceId, piece.DimX, piece.DimZ, piece.DimY);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        yRotate2.Blocks[x, piece.DimZ - 1 - z, y] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(yRotate2);
            // original #5 + 180 z
            KnownPiece yRotate3 = new KnownPiece(piece.PieceId, piece.DimX, piece.DimZ, piece.DimY);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        yRotate3.Blocks[piece.DimX - 1 - x, piece.DimZ - 1 - z, piece.DimY - 1 - y] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(yRotate3);
            // original #5 + 180 y
            KnownPiece yRotate4 = new KnownPiece(piece.PieceId, piece.DimX, piece.DimZ, piece.DimY);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        yRotate4.Blocks[piece.DimX - 1 - x, z, piece.DimY - 1 - y] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(yRotate4);
            // original #6 [top turned right]
            KnownPiece yRotate5 = new KnownPiece(piece.PieceId, piece.DimZ, piece.DimX, piece.DimY);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        yRotate5.Blocks[z, x, y] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(yRotate5);
            // original #6 + 180 y
            KnownPiece yRotate6 = new KnownPiece(piece.PieceId, piece.DimZ, piece.DimX, piece.DimY);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        yRotate6.Blocks[piece.DimZ - 1 - z, x, piece.DimY - 1 - y] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(yRotate6);
            // original #6 + 180 x
            KnownPiece yRotate7 = new KnownPiece(piece.PieceId, piece.DimZ, piece.DimX, piece.DimY);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        yRotate7.Blocks[z, piece.DimX - 1 - x, piece.DimY - 1 - y] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(yRotate7);
            // original #6 + 180 z
            KnownPiece yRotate8 = new KnownPiece(piece.PieceId, piece.DimZ, piece.DimX, piece.DimY);
            for (int z = 0; z < piece.DimZ; z++) {
                for (int y = 0; y < piece.DimY; y++) {
                    for (int x = 0; x < piece.DimX; x++) {
                        yRotate8.Blocks[piece.DimZ - 1 - z, piece.DimX - 1 - x, y] = piece.Blocks[x, y, z];
                    }
                }
            }
            retVal.Add(yRotate8);

            // make rotations unique
            retVal = retVal.GroupBy(pieceRotation => pieceRotation.Footprint).Select(group => group.First()).ToList();

            return retVal;
        }

        /// <summary>
        /// General idea:
        /// * create piece permutations (rotations; skip first one)
        /// * create all possible placements of said piece (one by one)
        /// * encode them as ulong and use & and ^ to validate solutions quickly
        /// * eliminate dead branches and hopefully find all solutions quite fast
        /// </summary>
        public class CubeSolver {

            public class PossibleSolution {
                public Tuple<int, int>[] PiecePermutationAndPlacement { get; private set; }
                public ulong CubeStatus { get; private set; }
                public byte[,,] Cube { get; private set; }

                public PossibleSolution(int permIndex, int placeIndex, ulong status) {
                    this.PiecePermutationAndPlacement = new Tuple<int, int>[KNOWN_PIECES.Length];
                    this.PiecePermutationAndPlacement[0] = new Tuple<int, int>(permIndex, placeIndex);
                    this.CubeStatus = status;

                }

                public PossibleSolution(PossibleSolution parent, int pieceIndex, int permIndex, int placeIndex, ulong status) {
                    this.PiecePermutationAndPlacement = new Tuple<int, int>[KNOWN_PIECES.Length];
                    for (int i = 0; i < parent.PiecePermutationAndPlacement.Length; i++) {
                        if (parent.PiecePermutationAndPlacement[i] != null) {
                            this.PiecePermutationAndPlacement[i] = parent.PiecePermutationAndPlacement[i];
                        }
                    }
                    this.PiecePermutationAndPlacement[pieceIndex] = new Tuple<int, int>(permIndex, placeIndex);
                    this.CubeStatus = status;
                }
            }

            List<BackgroundWorker> backgroundWorkers = new List<BackgroundWorker>();
            HashSet<PossibleSolution> solutions = new HashSet<PossibleSolution>();

            // every rotation/permutation per piece
            private KnownPiece[][] KnownPermutations = new KnownPiece[KNOWN_PIECES.Length][];
            // every placement per rotation per piece
            private Tuple<ulong, byte[,,]>[][][] KnownPlacements = new Tuple<ulong, byte[,,]>[KNOWN_PIECES.Length][][];

            public CubeSolver() {
                // first piece will not be rotated
                this.KnownPermutations[0] = new KnownPiece[1] { KNOWN_PIECES[0] };

                // create permutations (except for the first piece)
                for (int i = 1; i < KNOWN_PIECES.Length; i++) {
                    this.KnownPermutations[i] = GeneratePiecePermutations(KNOWN_PIECES[i]).ToArray();
                }

                // create all placements (ulong) for every permutation of every piece there is
                for (int pieceIndex = 0; pieceIndex < KNOWN_PIECES.Length; pieceIndex++) {
                    this.KnownPlacements[pieceIndex] = new Tuple<ulong, byte[,,]>[this.KnownPermutations[pieceIndex].Length][];
                    for (int permutationIndex = 0; permutationIndex < this.KnownPermutations[pieceIndex].Length; permutationIndex++) {
                        KnownPiece currentPermutation = this.KnownPermutations[pieceIndex][permutationIndex];
                        int xSteps = CUBE_X - currentPermutation.DimX + 1;
                        int ySteps = CUBE_Y - currentPermutation.DimY + 1;
                        int zSteps = CUBE_Z - currentPermutation.DimZ + 1;
                        int placementCount = xSteps * ySteps * zSteps;
                        this.KnownPlacements[pieceIndex][permutationIndex] = new Tuple<ulong, byte[,,]>[placementCount];
                        int count = 0;
                        // loop through all dimensions
                        for (byte z = 0; z <= (CUBE_Z - currentPermutation.DimZ); z++) {
                            for (byte y = 0; y <= (CUBE_Y - currentPermutation.DimY); y++) {
                                for (byte x = 0; x <= (CUBE_X - currentPermutation.DimX); x++) {
                                    byte[,,] trialCube = new byte[CUBE_X, CUBE_Y, CUBE_Z];
                                    PlacePiece(trialCube, x, y, z, currentPermutation);
                                    this.KnownPlacements[pieceIndex][permutationIndex][count] = CalculatePlacementTuple(trialCube);
                                    count++;
                                }
                            }
                        }
                    }
                }
            }

            private byte[,,] PlacePiece(byte[,,] currentCube, byte posX, byte posY, byte posZ, KnownPiece piece) {
                // place piece
                for (int pieceZ = 0; pieceZ < piece.DimZ; pieceZ++) {
                    for (int pieceY = 0; pieceY < piece.DimY; pieceY++) {
                        for (int pieceX = 0; pieceX < piece.DimX; pieceX++) {
                            // voxel in new piece
                            if (piece.Blocks[pieceX, pieceY, pieceZ] != 0) {
                                currentCube[posX + pieceX, posY + pieceY, posZ + pieceZ] = piece.Blocks[pieceX, pieceY, pieceZ];
                            }
                        }
                    }
                }
                return currentCube;
            }

            private Tuple<ulong, byte[,,]> CalculatePlacementTuple(byte[,,] currentCube) {
                ulong placementUlong = 0;
                ulong powerOfTwo = 1;
                byte[,,] cubeCopy = new byte[CUBE_X, CUBE_Y, CUBE_Z];
                for (int z = 0; z < CUBE_Z; z++) {
                    for (int y = 0; y < CUBE_Y; y++) {
                        for (int x = 0; x < CUBE_X; x++) {
                            if (currentCube[x, y, z] != 0) {
                                placementUlong += powerOfTwo;
                            }
                            powerOfTwo *= 2;
                            cubeCopy[x, y, z] = currentCube[x, y, z];
                        }
                    }
                }
                return new Tuple<ulong, byte[,,]>(placementUlong, currentCube);
            }

            private byte[,,] GetCubeFromPlacement(ulong[] placements) {
                byte[,,] cube = new byte[CUBE_X, CUBE_Y, CUBE_Z];
                for (int placeIndex = 0; placeIndex < placements.Length; placeIndex++) {
                    ulong powerOfTwo = (ulong)Math.Pow(2, CUBE_X * CUBE_Y * CUBE_Z - 1);
                    for (int x = CUBE_X - 1; x >= 0; x--) {
                        for (int y = CUBE_Y - 1; y >= 0; y--) {
                            for (int z = CUBE_Z - 1; z >= 0; z--) {
                                if (placements[placeIndex] >= powerOfTwo) {
                                    if (cube[x, y, z] != 0) {
                                        Console.WriteLine("Problem!");
                                    }
                                    else {
                                        cube[x, y, z] = (byte)(placeIndex + 1);
                                    }
                                    placements[placeIndex] -= powerOfTwo;
                                }
                                powerOfTwo /= 2;
                            }
                        }
                    }
                }
                return cube;
            }

            public void Run() {
                HashSet<PossibleSolution>[] validSolutionsPerStep = new HashSet<PossibleSolution>[KNOWN_PIECES.Length];

                // all placements of piece 1 are ways to valid solutions .. for now ;D
                validSolutionsPerStep[0] = new HashSet<PossibleSolution>();
                for (int placeIndex = 0; placeIndex < this.KnownPlacements[0][0].Length; placeIndex++) {
                    validSolutionsPerStep[0].Add(new PossibleSolution(0, placeIndex, this.KnownPlacements[0][0][placeIndex].Item1));
                }

                foreach (PossibleSolution initialStep in validSolutionsPerStep[0]) {
                    BackgroundWorker worker = new BackgroundWorker();
                    this.backgroundWorkers.Add(worker);
                    worker.DoWork += this.Worker_DoWork;
                    worker.RunWorkerCompleted += this.Worker_RunWorkerCompleted;
                    worker.RunWorkerAsync(initialStep);
                }
            }

            private void Worker_DoWork(object sender, DoWorkEventArgs e) {
                PossibleSolution startingPoint = e.Argument as PossibleSolution;
                HashSet<PossibleSolution>[] validSolutionsPerStep = new HashSet<PossibleSolution>[KNOWN_PIECES.Length];
                validSolutionsPerStep[0] = new HashSet<PossibleSolution>();
                validSolutionsPerStep[0].Add(startingPoint);

                // piece by piece
                for (int pieceIndex = 1; pieceIndex < KNOWN_PIECES.Length; pieceIndex++) {
                    validSolutionsPerStep[pieceIndex] = new HashSet<PossibleSolution>();
                    ulong tries = 0;
                    // against all previous solutions
                    foreach (PossibleSolution previousSolutionStep in validSolutionsPerStep[pieceIndex - 1]) {
                        // try all placements (including all permutations) of the current piece
                        for (int permutationIndex = 0; permutationIndex < this.KnownPlacements[pieceIndex].Length; permutationIndex++) {
                            for (int placementIndex = 0; placementIndex < this.KnownPlacements[pieceIndex][permutationIndex].Length; placementIndex++) {
                                // valid, no overlaps in bits
                                ulong placementBits = this.KnownPlacements[pieceIndex][permutationIndex][placementIndex].Item1;
                                if ((previousSolutionStep.CubeStatus & placementBits) == 0) {
                                    validSolutionsPerStep[pieceIndex].Add(new PossibleSolution(previousSolutionStep, pieceIndex, permutationIndex, placementIndex, previousSolutionStep.CubeStatus ^ placementBits));
                                }
                                else {
                                    // nothing ?!
                                }
                                tries++;
                            }
                        }
                    }
                    Console.WriteLine("Number of valid solutions (piece #" + pieceIndex + "): " + validSolutionsPerStep[pieceIndex].Count + " [tries: " + tries + "]");
                }

                // report back
                e.Result = validSolutionsPerStep;
            }

            private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
                BackgroundWorker worker = sender as BackgroundWorker;
                this.backgroundWorkers.Remove(worker);

                HashSet<PossibleSolution>[] allSteps = e.Result as HashSet<PossibleSolution>[];
                foreach (PossibleSolution possibleSolution in allSteps[KNOWN_PIECES.Length - 1]) {
                    this.solutions.Add(possibleSolution);
                }

                if (this.backgroundWorkers.Count > 0) {
                    // not done yet
                    return;
                }

                PossibleSolution solutionSteps = this.solutions.First();
                Console.WriteLine("Expected: " + ((ulong)Math.Pow(2, CUBE_X * CUBE_Y * CUBE_Z - 1) + ((ulong)Math.Pow(2, CUBE_X * CUBE_Y * CUBE_Z - 1) - 1)));
                Console.WriteLine("Solution: " + solutionSteps.CubeStatus);
                byte[,,] solutionCube = new byte[CUBE_X, CUBE_Y, CUBE_Z];
                ulong check = 0;
                for (int pieceIndex = 0; pieceIndex < KNOWN_PIECES.Length; pieceIndex++) {
                    Tuple<int, int> permAndPlace = solutionSteps.PiecePermutationAndPlacement[pieceIndex];
                    Console.WriteLine("Piece:" + (pieceIndex + 1) + " Permutation:" + permAndPlace.Item1 + " Placement:" + permAndPlace.Item2);
                    byte[,,] pieceSolution = this.KnownPlacements[pieceIndex][permAndPlace.Item1][permAndPlace.Item2].Item2;
                    check ^= this.KnownPlacements[pieceIndex][permAndPlace.Item1][permAndPlace.Item2].Item1;
                    for (int z = 0; z < CUBE_Z; z++) {
                        for (int y = 0; y < CUBE_Y; y++) {
                            for (int x = 0; x < CUBE_X; x++) {
                                if (pieceSolution[x, y, z] != 0) {
                                    solutionCube[x, y, z] = pieceSolution[x, y, z];
                                }
                            }
                        }
                    }
                }
                OutputCube(solutionCube, CUBE_X, CUBE_Y, CUBE_Z);

                byte[] solutionBytes = new byte[CUBE_X * CUBE_Y * CUBE_Z];
                for (int z = 0; z < CUBE_Z; z++) {
                    for (int y = 0; y < CUBE_Y; y++) {
                        for (int x = 0; x < CUBE_X; x++) {
                            solutionBytes[x + y * CUBE_X + z * CUBE_X * CUBE_Y] = solutionCube[x, y, z];
                        }
                    }
                }

                KnownPiece solutionPiece = new KnownPiece(0, CUBE_X, CUBE_Y, CUBE_Z, solutionBytes);
                List<KnownPiece> solutionRotations = GeneratePiecePermutations(solutionPiece);
                List<BigInteger> solutionSums = new List<BigInteger>();

                foreach (KnownPiece rotation in solutionRotations) {
                    StringBuilder digitsXYZ = new StringBuilder(new string('_', CUBE_X * CUBE_Y * CUBE_Z));
                    for (int z = 0; z < CUBE_Z; z++) {
                        for (int y = 0; y < CUBE_Y; y++) {
                            for (int x = 0; x < CUBE_X; x++) {
                                digitsXYZ[x + y * CUBE_X + z * CUBE_X * CUBE_Y] = Convert.ToChar(rotation.Blocks[x, y, z].ToString());
                            }
                        }
                    }
                    solutionSums.Add(BigInteger.Parse(String.Join("", digitsXYZ.ToString().Reverse())));
                }

                solutionSums.Sort();
                BigInteger hackerSum = BigInteger.Zero;
                foreach (BigInteger pieceSum in solutionSums) {
                    hackerSum += pieceSum;
                    Console.WriteLine(pieceSum);
                }

                Console.WriteLine("Hacker Solution: " + hackerSum);
            }

            private void OutputCube(byte[,,] cube, int maxX, int maxY, int maxZ) {
                for (int z = 0; z < maxZ; z++) {
                    for (int y = 0; y < maxY; y++) {
                        for (int x = 0; x < maxX; x++) {
                            Console.Write(cube[x, y, z]);
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine("---");
                }
                Console.WriteLine("*****");
            }
        }

        static void Main(string[] args) {
            CubeSolver solver = new CubeSolver();
            solver.Run();
            Console.WriteLine("Running.");
            Console.ReadKey(true);
        }

    }
}
