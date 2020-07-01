using System;
using System.Collections.Generic;

namespace ShreddedAndScrambled {
    public class MissingPiece {

        public int X { get; set; }
        public int Y { get; set; }
        public Dictionary<Direction, byte> Edges { get; set; }
        public List<Tuple<PuzzlePiece, int>> MatchingPieces { get; private set; }

        public MissingPiece() {
            this.MatchingPieces = new List<Tuple<PuzzlePiece, int>>();
        }
    }
}
