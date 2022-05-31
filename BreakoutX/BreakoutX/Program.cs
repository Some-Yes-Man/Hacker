﻿using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace BreakoutX {

    class Program {

        public class BlockInfo {
            public enum BlockType {
                UNKNOWN, RED, ORANGE, BLUE, VIOLET, INDIGO, GREEN, YELLOW, GREY, BLACK, DARKGREEN, IMMORTAL
            }

            int x { get; }
            int y { get; }
            bool immortal { get; }
            BlockType type { get; }

            public BlockInfo(int x, int y, bool immortal, BlockType type) {
                this.x = x;
                this.y = y;
                this.immortal = immortal;
                this.type = type;
            }
        }

        private const string level1 = "block 0 0 red.jpg  block 1 0 red.jpg  block 2 0 red.jpg  block 3 0 red.jpg  block 4 0 red.jpg  block 5 0 red.jpg  block 6 0 red.jpg  block 7 0 red.jpg  block 8 0 red.jpg  block 9 0 red.jpg  block 10 0 red.jpg  block 11 0 red.jpg  block 12 0 red.jpg  block 0 1 orange.jpg  block 1 1 orange.jpg  block 2 1 orange.jpg  block 3 1 orange.jpg  block 4 1 orange.jpg  block 5 1 orange.jpg  block 6 1 orange.jpg  block 7 1 orange.jpg  block 8 1 orange.jpg  block 9 1 orange.jpg  block 10 1 orange.jpg  block 11 1 orange.jpg  block 12 1 orange.jpg  block 0 2 yellow.jpg  block 1 2 yellow.jpg  block 2 2 yellow.jpg  block 3 2 yellow.jpg  block 4 2 yellow.jpg  block 5 2 yellow.jpg  block 6 2 yellow.jpg  block 7 2 yellow.jpg  block 8 2 yellow.jpg  block 9 2 yellow.jpg  block 10 2 yellow.jpg  block 11 2 yellow.jpg  block 12 2 yellow.jpg  block 0 3 green.jpg  block 1 3 green.jpg  block 2 3 green.jpg  block 3 3 green.jpg  block 4 3 green.jpg  block 5 3 green.jpg  block 6 3 green.jpg  block 7 3 green.jpg  block 8 3 green.jpg  block 9 3 green.jpg  block 10 3 green.jpg  block 11 3 green.jpg  block 12 3 green.jpg  block 0 4 blue.jpg  block 1 4 blue.jpg  block 2 4 blue.jpg  block 3 4 blue.jpg  block 4 4 blue.jpg  block 5 4 blue.jpg  block 6 4 blue.jpg  block 7 4 blue.jpg  block 8 4 blue.jpg  block 9 4 blue.jpg  block 10 4 blue.jpg  block 11 4 blue.jpg  block 12 4 blue.jpg  block 0 5 violet.jpg  block 1 5 violet.jpg  block 2 5 violet.jpg  block 3 5 violet.jpg  block 4 5 violet.jpg  block 5 5 violet.jpg  block 6 5 violet.jpg  block 7 5 violet.jpg  block 8 5 violet.jpg  block 9 5 violet.jpg  block 10 5 violet.jpg  block 11 5 violet.jpg  block 12 5 violet.jpg  block 0 6 indigo.jpg  block 1 6 indigo.jpg  block 2 6 indigo.jpg  block 3 6 indigo.jpg  block 4 6 indigo.jpg  wall 66  block 5 6 indigo.jpg  block 6 6 indigo.jpg  block 7 6 indigo.jpg  block 8 6 indigo.jpg  block 9 6 indigo.jpg  block 10 6 indigo.jpg  block 11 6 indigo.jpg  block 12 6 indigo.jpg  block 0 7 grey.jpg  block 1 7 grey.jpg  block 2 7 grey.jpg  block 3 7 grey.jpg  block 4 7 grey.jpg  block 5 7 grey.jpg  block 6 7 grey.jpg  block 7 7 grey.jpg  block 8 7 grey.jpg  block 9 7 grey.jpg  block 10 7 grey.jpg  block 11 7 grey.jpg  block 12 7 grey.jpg  block 0 8 black.jpg  block 1 8 black.jpg  block 2 8 black.jpg  block 3 8 black.jpg  block 4 8 black.jpg  block 5 8 black.jpg  block 6 8 black.jpg  block 7 8 black.jpg  block 8 8 black.jpg  block 9 8 black.jpg  block 10 8 black.jpg  block 11 8 black.jpg  block 12 8 black.jpg";
        private const string level2 = "block 0 0 red.jpg  block 5 0 black.jpg  block 6 0 black.jpg  block 7 0 black.jpg  block 12 0 orange.jpg  block 0 1 yellow.jpg  block 1 1 red.jpg  block 6 1 black.jpg  block 11 1 orange.jpg  block 12 1 green.jpg  block 0 2 violet.jpg  block 1 2 yellow.jpg  block 2 2 red.jpg  block 10 2 orange.jpg  block 11 2 green.jpg  block 12 2 blue.jpg  block 0 3 darkGreen.jpg wall 101 block 1 3 violet.jpg  block 2 3 yellow.jpg  block 3 3 red.jpg  block 9 3 orange.jpg  block 10 3 green.jpg  block 11 3 blue.jpg  block 12 3 indigo.jpg  block 0 4 black.jpg  block 1 4 darkGreen.jpg  block 2 4 violet.jpg  block 3 4 yellow.jpg  block 4 4 red.jpg  block 8 4 orange.jpg  block 9 4 green.jpg  block 10 4 blue.jpg  block 11 4 indigo.jpg  block 12 4 black.jpg  block 0 5 black.jpg  block 1 5 black.jpg  block 2 5 darkGreen.jpg  block 3 5 violet.jpg  block 4 5 yellow.jpg  block 5 5 red.jpg  block 7 5 orange.jpg  block 8 5 green.jpg  block 9 5 blue.jpg  block 10 5 indigo.jpg  block 11 5 black.jpg  block 12 5 black.jpg  block 0 6 black.jpg  block 1 6 black.jpg  block 2 6 black.jpg  block 3 6 darkGreen.jpg  block 4 6 violet.jpg  block 5 6 yellow.jpg  block 6 6 black.jpg  block 7 6 green.jpg  block 8 6 blue.jpg  block 9 6 indigo.jpg  block 10 6 black.jpg  block 11 6 black.jpg  block 12 6 black.jpg  block 0 7 black.jpg  block 1 7 black.jpg  block 2 7 darkGreen.jpg  block 3 7 violet.jpg  block 4 7 yellow.jpg  block 5 7 orange.jpg  block 7 7 red.jpg  block 8 7 green.jpg  block 9 7 blue.jpg  block 10 7 indigo.jpg  block 11 7 black.jpg  block 12 7 black.jpg  block 0 8 black.jpg  block 1 8 darkGreen.jpg  block 2 8 violet.jpg  block 3 8 yellow.jpg  block 4 8 orange.jpg  block 8 8 red.jpg  block 9 8 green.jpg  block 10 8 blue.jpg  block 11 8 indigo.jpg  block 12 8 black.jpg  block 0 9 darkGreen.jpg  block 1 9 violet.jpg  block 2 9 yellow.jpg  block 3 9 orange.jpg  block 9 9 red.jpg  block 10 9 green.jpg  block 11 9 blue.jpg  block 12 9 indigo.jpg  block 0 10 violet.jpg  block 1 10 yellow.jpg  block 2 10 orange.jpg  block 10 10 red.jpg  block 11 10 green.jpg  block 12 10 blue.jpg  block 0 11 yellow.jpg  block 1 11 orange.jpg  block 11 11 red.jpg  block 12 11 green.jpg  block 0 12 orange.jpg  block 12 12 red.jpg";
        private const string level3 = "block 0 0 black.jpg  block 2 0 grey.jpg  block 4 0 blue.jpg  block 6 0 green.jpg  block 8 0 orange.jpg  block 10 0 yellow.jpg  block 12 0 red.jpg  block 0 1 black.jpg  block 2 1 grey.jpg  block 4 1 blue.jpg  block 6 1 green.jpg  block 8 1 orange.jpg  block 10 1 yellow.jpg  block 12 1 red.jpg  block 0 2 black.jpg  block 2 2 grey.jpg  block 4 2 blue.jpg  wall 100 block 6 2 green.jpg  block 8 2 orange.jpg  block 10 2 yellow.jpg  block 12 2 red.jpg  block 0 3 black.jpg  block 2 3 grey.jpg  block 4 3 blue.jpg  block 6 3 green.jpg  block 8 3 orange.jpg  block 10 3 yellow.jpg  block 12 3 red.jpg  block 0 4 black.jpg  block 2 4 grey.jpg  block 4 4 blue.jpg  block 6 4 green.jpg  block 8 4 orange.jpg  block 10 4 yellow.jpg  block 12 4 red.jpg  block 0 5 black.jpg  block 2 5 grey.jpg  block 4 5 blue.jpg  block 6 5 green.jpg  block 8 5 orange.jpg  block 10 5 yellow.jpg  block 12 5 red.jpg  block 0 6 black.jpg  block 2 6 grey.jpg  block 4 6 blue.jpg  block 6 6 green.jpg  block 8 6 orange.jpg  block 10 6 yellow.jpg  block 12 6 red.jpg  block 0 7 black.jpg  block 2 7 grey.jpg  block 4 7 blue.jpg  block 6 7 green.jpg  block 8 7 orange.jpg  block 10 7 yellow.jpg  block 12 7 red.jpg  immortalBlock 0 8 immortalBlock.jpg  immortalBlock 2 8 immortalBlock.jpg  immortalBlock 4 8 immortalBlock.jpg  immortalBlock 6 8 immortalBlock.jpg  immortalBlock 8 8 immortalBlock.jpg  immortalBlock 10 8 immortalBlock.jpg  immortalBlock 12 8 immortalBlock.jpg";
        private const string level4 = "block 0 5 orange.jpg  block 2 5 orange.jpg  block 4 5 orange.jpg  block 6 5 orange.jpg  block 8 5 orange.jpg  block 10 5 orange.jpg  block 12 5 orange.jpg  block 1 6 orange.jpg  block 3 6 orange.jpg  block 5 6 orange.jpg  block 7 6 orange.jpg  block 9 6 orange.jpg  block 11 6 orange.jpg  block 0 7 blue.jpg  block 1 7 blue.jpg  block 2 7 blue.jpg  block 3 7 blue.jpg wall 121 block 4 7 blue.jpg  block 5 7 blue.jpg  block 6 7 blue.jpg  block 7 7 blue.jpg  block 8 7 blue.jpg  block 9 7 blue.jpg  block 10 7 blue.jpg  block 11 7 blue.jpg  block 12 7 blue.jpg  block 0 8 yellow.jpg  block 1 8 yellow.jpg  block 2 8 yellow.jpg  block 3 8 yellow.jpg  block 4 8 yellow.jpg  block 5 8 yellow.jpg  block 6 8 yellow.jpg  block 7 8 yellow.jpg  block 8 8 yellow.jpg  block 9 8 yellow.jpg  block 10 8 yellow.jpg  block 11 8 yellow.jpg  block 12 8 yellow.jpg  block 0 9 black.jpg  block 1 9 black.jpg  block 2 9 black.jpg  block 3 9 black.jpg  block 4 9 black.jpg  block 5 9 black.jpg  block 6 9 black.jpg  block 7 9 black.jpg  block 8 9 black.jpg  block 9 9 black.jpg  block 10 9 black.jpg  block 11 9 black.jpg  block 12 9 black.jpg  block 0 10 red.jpg  block 1 10 red.jpg  block 2 10 red.jpg  block 3 10 red.jpg  block 4 10 red.jpg  block 5 10 red.jpg  block 6 10 red.jpg  block 7 10 red.jpg  block 8 10 red.jpg  block 9 10 red.jpg  block 10 10 red.jpg  block 11 10 red.jpg  block 12 10 red.jpg  block 1 11 green.jpg  block 3 11 green.jpg  block 5 11 green.jpg  block 7 11 green.jpg  block 9 11 green.jpg  block 11 11 green.jpg  block 0 12 green.jpg  block 2 12 green.jpg  block 4 12 green.jpg  block 6 12 green.jpg  block 8 12 green.jpg  block 10 12 green.jpg  block 12 12 green.jpg";
        private const string level5 = "block 0 1 green.jpg  block 1 1 green.jpg  block 2 1 green.jpg  block 6 1 blue.jpg  block 8 1 red.jpg  block 10 1 red.jpg  block 12 1 indigo.jpg  block 2 2 green.jpg  block 6 2 blue.jpg  block 8 2 red.jpg  block 10 2 red.jpg  block 12 2 indigo.jpg  block 0 3 green.jpg  block 1 3 green.jpg  block 2 3 green.jpg  block 6 3 blue.jpg  block 8 3 red.jpg  block 9 3 red.jpg  block 10 3 red.jpg  block 12 3 indigo.jpg  block 2 4 green.jpg wall 97 block 6 4 blue.jpg  block 10 4 red.jpg  block 12 4 indigo.jpg  block 0 5 green.jpg  block 1 5 green.jpg  block 2 5 green.jpg  immortalBlock 4 5 immortalBlock.jpg  block 6 5 blue.jpg  block 10 5 red.jpg  block 12 5 indigo.jpg  block 0 8 darkGreen.jpg  block 1 8 darkGreen.jpg  block 2 8 darkGreen.jpg  block 4 8 orange.jpg  block 5 8 orange.jpg  block 6 8 orange.jpg  block 8 8 violet.jpg  block 9 8 violet.jpg  block 10 8 violet.jpg  block 0 9 darkGreen.jpg  block 4 9 orange.jpg  block 6 9 orange.jpg  block 10 9 violet.jpg  block 0 10 darkGreen.jpg  block 1 10 darkGreen.jpg  block 2 10 darkGreen.jpg  block 4 10 orange.jpg  block 5 10 orange.jpg  block 6 10 orange.jpg  block 9 10 violet.jpg  block 2 11 darkGreen.jpg  block 6 11 orange.jpg  block 8 11 violet.jpg  block 0 12 darkGreen.jpg  block 1 12 darkGreen.jpg  block 2 12 darkGreen.jpg  block 6 12 orange.jpg  block 8 12 violet.jpg  block 9 12 violet.jpg  block 10 12 violet.jpg  block 0 14 grey.jpg  block 1 14 grey.jpg  block 2 14 grey.jpg  block 4 14 black.jpg  block 5 14 black.jpg  block 6 14 black.jpg  block 8 14 yellow.jpg  block 9 14 yellow.jpg  block 10 14 yellow.jpg  block 0 15 grey.jpg  block 4 15 black.jpg  block 10 15 yellow.jpg  block 0 16 grey.jpg  block 1 16 grey.jpg  block 2 16 grey.jpg  block 4 16 black.jpg  block 5 16 black.jpg  block 6 16 black.jpg  block 8 16 yellow.jpg  block 9 16 yellow.jpg  block 10 16 yellow.jpg  block 0 17 grey.jpg  block 2 17 grey.jpg  block 6 17 black.jpg  block 10 17 yellow.jpg  block 0 18 grey.jpg  block 1 18 grey.jpg  block 2 18 grey.jpg  block 4 18 black.jpg  block 5 18 black.jpg  block 6 18 black.jpg  block 8 18 yellow.jpg  block 9 18 yellow.jpg  block 10 18 yellow.jpg";
        private const string level6 = "block 1 0 blue.jpg  block 5 0 blue.jpg  block 2 1 blue.jpg  block 6 1 yellow.jpg  block 9 1 blue.jpg  block 3 2 blue.jpg  block 5 2 yellow.jpg  block 6 2 yellow.jpg  block 7 2 yellow.jpg  block 11 2 blue.jpg  block 0 3 blue.jpg  block 4 3 yellow.jpg  block 5 3 yellow.jpg  block 6 3 yellow.jpg  block 7 3 yellow.jpg  block 8 3 yellow.jpg  block 3 4 yellow.jpg  block 4 4 yellow.jpg  block 5 4 yellow.jpg  block 6 4 yellow.jpg  block 7 4 yellow.jpg  block 8 4 yellow.jpg wall 112 block 9 4 yellow.jpg  block 12 4 blue.jpg  block 2 5 yellow.jpg  block 3 5 yellow.jpg  block 4 5 yellow.jpg  block 5 5 yellow.jpg  block 6 5 yellow.jpg  block 7 5 yellow.jpg  block 8 5 yellow.jpg  block 9 5 yellow.jpg  block 10 5 yellow.jpg  immortalBlock 6 6 immortalBlock.jpg  block 1 7 blue.jpg  immortalBlock 6 7 immortalBlock.jpg  block 12 7 blue.jpg  immortalBlock 6 8 immortalBlock.jpg  block 0 9 blue.jpg  immortalBlock 6 9 immortalBlock.jpg  immortalBlock 6 10 immortalBlock.jpg  block 11 10 blue.jpg  block 1 11 blue.jpg  immortalBlock 6 11 immortalBlock.jpg  immortalBlock 6 12 immortalBlock.jpg  block 0 13 blue.jpg  block 3 13 black.jpg  immortalBlock 6 13 immortalBlock.jpg  block 11 13 blue.jpg  block 4 14 black.jpg  immortalBlock 6 14 immortalBlock.jpg  block 5 15 black.jpg  immortalBlock 6 15 immortalBlock.jpg  block 12 15 blue.jpg  block 1 17 blue.jpg";
        private const string level7 = "block 1 1 violet.jpg  block 2 1 violet.jpg  block 3 1 violet.jpg  block 4 1 violet.jpg  block 5 1 violet.jpg  block 6 1 violet.jpg  block 7 1 violet.jpg  block 8 1 violet.jpg  block 9 1 violet.jpg  block 10 1 violet.jpg  block 11 1 violet.jpg  block 1 2 violet.jpg  block 6 2 red.jpg  block 11 2 violet.jpg  block 1 3 violet.jpg  block 4 3 red.jpg  block 5 3 red.jpg  block 6 3 blue.jpg  block 7 3 red.jpg  block 8 3 red.jpg  block 11 3 violet.jpg  block 1 4 violet.jpg  block 3 4 red.jpg wall 101 block 5 4 blue.jpg  immortalBlock 6 4 immortalBlock.jpg  block 7 4 blue.jpg  block 9 4 red.jpg  block 11 4 violet.jpg  block 1 5 violet.jpg  block 2 5 red.jpg  block 4 5 blue.jpg  immortalBlock 6 5 immortalBlock.jpg  block 8 5 blue.jpg  block 10 5 red.jpg  block 11 5 violet.jpg  block 1 6 violet.jpg  block 2 6 red.jpg  block 3 6 blue.jpg  immortalBlock 5 6 immortalBlock.jpg  immortalBlock 7 6 immortalBlock.jpg  block 9 6 blue.jpg  block 10 6 red.jpg  block 11 6 violet.jpg  block 1 7 violet.jpg  block 2 7 red.jpg  block 3 7 blue.jpg  immortalBlock 5 7 immortalBlock.jpg  immortalBlock 7 7 immortalBlock.jpg  block 9 7 blue.jpg  block 10 7 red.jpg  block 11 7 violet.jpg  block 1 8 violet.jpg  block 2 8 red.jpg  block 3 8 blue.jpg  immortalBlock 5 8 immortalBlock.jpg  immortalBlock 7 8 immortalBlock.jpg  block 9 8 blue.jpg  block 10 8 red.jpg  block 11 8 violet.jpg  block 1 9 violet.jpg  block 2 9 red.jpg  block 4 9 blue.jpg  immortalBlock 6 9 immortalBlock.jpg  block 8 9 blue.jpg  block 10 9 red.jpg  block 11 9 violet.jpg  block 1 10 violet.jpg  block 2 10 red.jpg  block 5 10 blue.jpg  immortalBlock 6 10 immortalBlock.jpg  block 7 10 blue.jpg  block 10 10 red.jpg  block 11 10 violet.jpg  block 1 11 violet.jpg  block 3 11 red.jpg  block 6 11 blue.jpg  block 9 11 red.jpg  block 11 11 violet.jpg  block 1 12 violet.jpg  block 4 12 red.jpg  block 5 12 red.jpg  block 7 12 red.jpg  block 8 12 red.jpg  block 11 12 violet.jpg  block 1 13 violet.jpg  block 6 13 red.jpg  block 11 13 violet.jpg  block 1 14 violet.jpg  block 2 14 violet.jpg  block 3 14 violet.jpg  block 4 14 violet.jpg  block 5 14 violet.jpg  block 6 14 violet.jpg  block 7 14 violet.jpg  block 8 14 violet.jpg  block 9 14 violet.jpg  block 10 14 violet.jpg  block 11 14 violet.jpg";
        private const string level8 = "block 6 0 yellow.jpg  block 7 0 yellow.jpg  block 8 0 yellow.jpg  block 5 1 yellow.jpg  block 6 1 yellow.jpg  block 7 1 yellow.jpg  block 8 1 yellow.jpg  block 9 1 yellow.jpg  block 4 2 yellow.jpg  block 5 2 yellow.jpg  block 6 2 yellow.jpg  block 7 2 yellow.jpg  block 8 2 yellow.jpg  block 9 2 yellow.jpg  block 10 2 yellow.jpg  block 4 3 yellow.jpg  block 5 3 yellow.jpg  block 6 3 yellow.jpg  block 7 3 yellow.jpg  block 8 3 yellow.jpg  block 9 3 yellow.jpg  block 10 3 yellow.jpg wall 97 block 5 4 black.jpg  block 6 4 black.jpg  block 7 4 black.jpg  block 8 4 black.jpg  block 9 4 black.jpg  block 1 5 grey.jpg  block 3 5 grey.jpg  block 5 5 black.jpg  block 6 5 orange.jpg  block 7 5 black.jpg  block 8 5 orange.jpg  block 9 5 black.jpg  block 1 6 grey.jpg  block 3 6 grey.jpg  block 5 6 black.jpg  block 6 6 black.jpg  block 7 6 black.jpg  block 8 6 black.jpg  block 9 6 black.jpg  block 1 7 violet.jpg  block 2 7 violet.jpg  block 3 7 violet.jpg  block 5 7 black.jpg  block 6 7 black.jpg  block 7 7 orange.jpg  block 8 7 black.jpg  block 9 7 black.jpg  block 2 8 violet.jpg  block 5 8 black.jpg  block 6 8 black.jpg  block 7 8 black.jpg  block 8 8 black.jpg  block 9 8 black.jpg  block 2 9 violet.jpg  block 7 9 black.jpg  block 2 10 black.jpg  block 3 10 black.jpg  block 4 10 black.jpg  block 5 10 black.jpg  block 6 10 black.jpg  block 7 10 black.jpg  block 8 10 black.jpg  block 9 10 black.jpg  block 10 10 black.jpg  block 11 10 black.jpg  block 2 11 violet.jpg  block 7 11 black.jpg  block 2 12 violet.jpg  block 7 12 black.jpg  block 2 13 violet.jpg  block 6 13 black.jpg  block 7 13 black.jpg  block 8 13 black.jpg  block 2 14 violet.jpg  block 5 14 black.jpg  block 6 14 black.jpg  block 8 14 black.jpg  block 9 14 black.jpg  block 2 15 violet.jpg  block 5 15 black.jpg  block 6 15 black.jpg  block 8 15 black.jpg  block 9 15 black.jpg  block 2 16 violet.jpg  block 5 16 black.jpg  block 6 16 black.jpg  block 8 16 black.jpg  block 9 16 black.jpg  block 5 17 black.jpg  block 9 17 black.jpg  block 4 18 darkGreen.jpg  block 5 18 darkGreen.jpg  block 6 18 darkGreen.jpg  block 7 18 darkGreen.jpg  block 8 18 darkGreen.jpg  block 9 18 darkGreen.jpg  block 10 18 darkGreen.jpg";
        private const string level9 = "immortalBlock 1 3 immortalBlock.jpg  immortalBlock 4 3 immortalBlock.jpg  immortalBlock 5 3 immortalBlock.jpg  immortalBlock 6 3 immortalBlock.jpg  immortalBlock 7 3 immortalBlock.jpg  immortalBlock 8 3 immortalBlock.jpg  immortalBlock 11 3 immortalBlock.jpg  immortalBlock 1 4 immortalBlock.jpg  immortalBlock 11 4 immortalBlock.jpg  immortalBlock 1 5 immortalBlock.jpg  immortalBlock 11 5 immortalBlock.jpg  immortalBlock 1 6 immortalBlock.jpg  immortalBlock 11 6 immortalBlock.jpg wall 116 immortalBlock 1 7 immortalBlock.jpg  block 2 7 indigo.jpg  block 3 7 indigo.jpg  block 4 7 indigo.jpg  block 5 7 indigo.jpg  block 6 7 indigo.jpg  block 7 7 indigo.jpg  block 8 7 indigo.jpg  block 9 7 indigo.jpg  block 10 7 indigo.jpg  immortalBlock 11 7 immortalBlock.jpg  immortalBlock 1 8 immortalBlock.jpg  block 2 8 violet.jpg  block 3 8 violet.jpg  block 4 8 violet.jpg  block 5 8 violet.jpg  block 6 8 violet.jpg  block 7 8 violet.jpg  block 8 8 violet.jpg  block 9 8 violet.jpg  block 10 8 violet.jpg  immortalBlock 11 8 immortalBlock.jpg  immortalBlock 1 9 immortalBlock.jpg  block 2 9 blue.jpg  block 3 9 blue.jpg  block 4 9 blue.jpg  block 5 9 blue.jpg  block 6 9 blue.jpg  block 7 9 blue.jpg  block 8 9 blue.jpg  block 9 9 blue.jpg  block 10 9 blue.jpg  immortalBlock 11 9 immortalBlock.jpg  immortalBlock 1 10 immortalBlock.jpg  block 2 10 green.jpg  block 3 10 green.jpg  block 4 10 green.jpg  block 5 10 green.jpg  block 6 10 green.jpg  block 7 10 green.jpg  block 8 10 green.jpg  block 9 10 green.jpg  block 10 10 green.jpg  immortalBlock 11 10 immortalBlock.jpg  immortalBlock 1 11 immortalBlock.jpg  block 2 11 yellow.jpg  block 3 11 yellow.jpg  block 4 11 yellow.jpg  block 5 11 yellow.jpg  block 6 11 yellow.jpg  block 7 11 yellow.jpg  block 8 11 yellow.jpg  block 9 11 yellow.jpg  block 10 11 yellow.jpg  immortalBlock 11 11 immortalBlock.jpg  immortalBlock 1 12 immortalBlock.jpg  block 2 12 orange.jpg  block 3 12 orange.jpg  block 4 12 orange.jpg  block 5 12 orange.jpg  block 6 12 orange.jpg  block 7 12 orange.jpg  block 8 12 orange.jpg  block 9 12 orange.jpg  block 10 12 orange.jpg  immortalBlock 11 12 immortalBlock.jpg  immortalBlock 1 13 immortalBlock.jpg  block 2 13 red.jpg  block 3 13 red.jpg  block 4 13 red.jpg  block 5 13 red.jpg  block 6 13 red.jpg  block 7 13 red.jpg  block 8 13 red.jpg  block 9 13 red.jpg  block 10 13 red.jpg  immortalBlock 11 13 immortalBlock.jpg  immortalBlock 1 14 immortalBlock.jpg  immortalBlock 2 14 immortalBlock.jpg  immortalBlock 3 14 immortalBlock.jpg  immortalBlock 4 14 immortalBlock.jpg  immortalBlock 5 14 immortalBlock.jpg  immortalBlock 6 14 immortalBlock.jpg  immortalBlock 7 14 immortalBlock.jpg  immortalBlock 8 14 immortalBlock.jpg  immortalBlock 9 14 immortalBlock.jpg  immortalBlock 10 14 immortalBlock.jpg  immortalBlock 11 14 immortalBlock.jpg";
        private const string level10 = "immortalBlock 3 8 immortalBlock.jpg  immortalBlock 4 8 immortalBlock.jpg  immortalBlock 5 8 immortalBlock.jpg  block 6 8 black.jpg  immortalBlock 7 8 immortalBlock.jpg  immortalBlock 8 8 immortalBlock.jpg  immortalBlock 9 8 immortalBlock.jpg  immortalBlock 3 9 immortalBlock.jpg  block 4 9 yellow.jpg  immortalBlock 5 9 immortalBlock.jpg  block 6 9 black.jpg  immortalBlock 7 9 immortalBlock.jpg  block 8 9 green.jpg  immortalBlock 9 9 immortalBlock.jpg  immortalBlock 3 10 immortalBlock.jpg wall 103 block 4 10 yellow.jpg  immortalBlock 5 10 immortalBlock.jpg  block 6 10 black.jpg  immortalBlock 7 10 immortalBlock.jpg  block 8 10 green.jpg  immortalBlock 9 10 immortalBlock.jpg  block 0 14 black.jpg  immortalBlock 1 14 immortalBlock.jpg  immortalBlock 2 14 immortalBlock.jpg  immortalBlock 3 14 immortalBlock.jpg  block 4 14 black.jpg  immortalBlock 5 14 immortalBlock.jpg  immortalBlock 6 14 immortalBlock.jpg  immortalBlock 7 14 immortalBlock.jpg  block 8 14 black.jpg  immortalBlock 9 14 immortalBlock.jpg  immortalBlock 10 14 immortalBlock.jpg  immortalBlock 11 14 immortalBlock.jpg  block 12 14 black.jpg  block 0 15 black.jpg  immortalBlock 1 15 immortalBlock.jpg  block 2 15 violet.jpg  immortalBlock 3 15 immortalBlock.jpg  block 4 15 black.jpg  immortalBlock 5 15 immortalBlock.jpg  block 6 15 darkGreen.jpg  immortalBlock 7 15 immortalBlock.jpg  block 8 15 black.jpg  immortalBlock 9 15 immortalBlock.jpg  block 10 15 red.jpg  immortalBlock 11 15 immortalBlock.jpg  block 12 15 black.jpg  block 0 16 black.jpg  immortalBlock 1 16 immortalBlock.jpg  block 2 16 violet.jpg  immortalBlock 3 16 immortalBlock.jpg  block 4 16 black.jpg  immortalBlock 5 16 immortalBlock.jpg  block 6 16 darkGreen.jpg  immortalBlock 7 16 immortalBlock.jpg  block 8 16 black.jpg  immortalBlock 9 16 immortalBlock.jpg  block 10 16 red.jpg  immortalBlock 11 16 immortalBlock.jpg  block 12 16 black.jpg";
        private const string level11 = "block 0 0 blue.jpg  block 1 0 blue.jpg  block 2 0 blue.jpg  block 3 0 blue.jpg  block 4 0 blue.jpg  block 5 0 blue.jpg  block 6 0 blue.jpg  block 7 0 blue.jpg  block 8 0 yellow.jpg  block 9 0 blue.jpg  block 10 0 yellow.jpg  block 11 0 blue.jpg  block 12 0 yellow.jpg  block 0 1 blue.jpg  block 1 1 blue.jpg  block 2 1 blue.jpg  block 3 1 grey.jpg  block 4 1 grey.jpg  block 5 1 grey.jpg  block 6 1 grey.jpg  block 7 1 blue.jpg  block 8 1 blue.jpg  block 9 1 yellow.jpg  block 10 1 yellow.jpg wall 111 block 11 1 yellow.jpg  block 12 1 blue.jpg  block 0 2 blue.jpg  block 1 2 blue.jpg  block 2 2 grey.jpg  block 3 2 grey.jpg  block 4 2 grey.jpg  block 5 2 grey.jpg  block 6 2 grey.jpg  block 7 2 blue.jpg  block 8 2 yellow.jpg  block 9 2 yellow.jpg  block 10 2 yellow.jpg  block 11 2 yellow.jpg  block 12 2 yellow.jpg  block 0 3 blue.jpg  block 1 3 blue.jpg  block 2 3 grey.jpg  block 3 3 grey.jpg  block 4 3 grey.jpg  block 5 3 grey.jpg  block 6 3 blue.jpg  block 7 3 blue.jpg  block 8 3 blue.jpg  block 9 3 yellow.jpg  block 10 3 yellow.jpg  block 11 3 yellow.jpg  block 12 3 blue.jpg  block 0 4 blue.jpg  block 1 4 blue.jpg  block 2 4 grey.jpg  block 3 4 blue.jpg  block 4 4 blue.jpg  block 5 4 blue.jpg  block 6 4 blue.jpg  block 7 4 blue.jpg  block 8 4 yellow.jpg  block 9 4 blue.jpg  block 10 4 yellow.jpg  block 11 4 blue.jpg  block 12 4 yellow.jpg  block 0 5 blue.jpg  block 1 5 blue.jpg  block 2 5 blue.jpg  block 3 5 blue.jpg  block 4 5 blue.jpg  block 5 5 blue.jpg  block 6 5 blue.jpg  block 7 5 blue.jpg  block 8 5 blue.jpg  block 9 5 blue.jpg  block 10 5 blue.jpg  block 11 5 blue.jpg  block 12 5 blue.jpg  block 0 6 blue.jpg  block 1 6 blue.jpg  block 2 6 blue.jpg  block 3 6 blue.jpg  block 4 6 blue.jpg  block 5 6 blue.jpg  block 6 6 blue.jpg  block 7 6 blue.jpg  block 8 6 blue.jpg  block 9 6 blue.jpg  block 10 6 blue.jpg  block 11 6 blue.jpg  block 12 6 blue.jpg  block 0 7 blue.jpg  block 1 7 blue.jpg  block 2 7 blue.jpg  block 3 7 blue.jpg  block 4 7 blue.jpg  block 5 7 blue.jpg  block 6 7 blue.jpg  block 7 7 blue.jpg  block 8 7 blue.jpg  block 9 7 blue.jpg  block 10 7 blue.jpg  block 11 7 blue.jpg  block 12 7 blue.jpg  block 0 8 blue.jpg  block 1 8 blue.jpg  block 2 8 blue.jpg  block 3 8 blue.jpg  block 4 8 blue.jpg  block 5 8 blue.jpg  block 6 8 blue.jpg  block 7 8 blue.jpg  block 8 8 blue.jpg  block 9 8 blue.jpg  block 10 8 blue.jpg  block 11 8 blue.jpg  block 12 8 blue.jpg  block 0 9 blue.jpg  block 1 9 blue.jpg  block 2 9 blue.jpg  block 3 9 blue.jpg  block 4 9 blue.jpg  block 5 9 blue.jpg  block 6 9 blue.jpg  block 7 9 blue.jpg  block 8 9 blue.jpg  block 9 9 blue.jpg  block 10 9 blue.jpg  block 11 9 blue.jpg  block 12 9 blue.jpg  block 0 10 blue.jpg  block 1 10 blue.jpg  block 2 10 blue.jpg  block 3 10 blue.jpg  block 4 10 blue.jpg  block 5 10 blue.jpg  block 6 10 blue.jpg  block 7 10 blue.jpg  block 8 10 blue.jpg  block 9 10 blue.jpg  block 10 10 blue.jpg  block 11 10 blue.jpg  block 12 10 blue.jpg  block 0 11 blue.jpg  block 1 11 blue.jpg  block 2 11 blue.jpg  block 3 11 darkGreen.jpg  block 4 11 blue.jpg  block 5 11 blue.jpg  block 6 11 blue.jpg  block 7 11 blue.jpg  block 8 11 blue.jpg  block 9 11 blue.jpg  block 10 11 blue.jpg  block 11 11 blue.jpg  block 12 11 blue.jpg  block 0 12 blue.jpg  block 1 12 blue.jpg  block 2 12 darkGreen.jpg  block 3 12 darkGreen.jpg  block 4 12 darkGreen.jpg  block 5 12 blue.jpg  block 6 12 blue.jpg  block 7 12 blue.jpg  block 8 12 blue.jpg  block 9 12 blue.jpg  block 10 12 blue.jpg  block 11 12 blue.jpg  block 12 12 blue.jpg  block 0 13 blue.jpg  block 1 13 darkGreen.jpg  block 2 13 darkGreen.jpg  block 3 13 darkGreen.jpg  block 4 13 darkGreen.jpg  block 5 13 darkGreen.jpg  block 6 13 blue.jpg  block 7 13 blue.jpg  block 8 13 blue.jpg  block 9 13 darkGreen.jpg  block 10 13 darkGreen.jpg  block 11 13 blue.jpg  block 12 13 blue.jpg  block 0 14 darkGreen.jpg  block 1 14 darkGreen.jpg  block 2 14 darkGreen.jpg  block 3 14 darkGreen.jpg  block 4 14 darkGreen.jpg  block 5 14 darkGreen.jpg  block 6 14 darkGreen.jpg  block 7 14 blue.jpg  block 8 14 darkGreen.jpg  block 9 14 darkGreen.jpg  block 10 14 darkGreen.jpg  block 11 14 darkGreen.jpg  block 12 14 blue.jpg  block 0 15 darkGreen.jpg  block 1 15 darkGreen.jpg  block 2 15 darkGreen.jpg  block 3 15 darkGreen.jpg  block 4 15 darkGreen.jpg  block 5 15 darkGreen.jpg  block 6 15 darkGreen.jpg  block 7 15 darkGreen.jpg  block 8 15 darkGreen.jpg  block 9 15 darkGreen.jpg  block 10 15 darkGreen.jpg  block 11 15 darkGreen.jpg  block 12 15 darkGreen.jpg  block 0 16 darkGreen.jpg  block 1 16 darkGreen.jpg  block 2 16 darkGreen.jpg  block 3 16 darkGreen.jpg  block 4 16 darkGreen.jpg  block 5 16 darkGreen.jpg  block 6 16 darkGreen.jpg  block 7 16 darkGreen.jpg  block 8 16 darkGreen.jpg  block 9 16 darkGreen.jpg  block 10 16 darkGreen.jpg  block 11 16 darkGreen.jpg  block 12 16 darkGreen.jpg";
        private const string level12 = "block 0 0 grey.jpg  block 1 0 grey.jpg  block 2 0 grey.jpg  block 3 0 grey.jpg  block 4 0 grey.jpg  block 5 0 grey.jpg  block 6 0 grey.jpg  block 7 0 grey.jpg  block 8 0 grey.jpg  block 9 0 grey.jpg  block 10 0 grey.jpg  block 11 0 grey.jpg  immortalBlock 0 2 immortalBlock.jpg  immortalBlock 1 2 immortalBlock.jpg  immortalBlock 2 2 immortalBlock.jpg  immortalBlock 3 2 immortalBlock.jpg  immortalBlock 4 2 immortalBlock.jpg  immortalBlock 5 2 immortalBlock.jpg  immortalBlock 6 2 immortalBlock.jpg wall 98 immortalBlock 7 2 immortalBlock.jpg  immortalBlock 8 2 immortalBlock.jpg  immortalBlock 9 2 immortalBlock.jpg  immortalBlock 10 2 immortalBlock.jpg  immortalBlock 11 2 immortalBlock.jpg  block 0 3 yellow.jpg  block 1 3 yellow.jpg  block 2 3 yellow.jpg  block 3 3 yellow.jpg  block 4 3 yellow.jpg  block 5 3 yellow.jpg  block 6 3 yellow.jpg  block 7 3 yellow.jpg  block 8 3 yellow.jpg  block 9 3 yellow.jpg  block 10 3 yellow.jpg  block 11 3 yellow.jpg  immortalBlock 1 5 immortalBlock.jpg  immortalBlock 2 5 immortalBlock.jpg  immortalBlock 3 5 immortalBlock.jpg  immortalBlock 4 5 immortalBlock.jpg  immortalBlock 5 5 immortalBlock.jpg  immortalBlock 6 5 immortalBlock.jpg  immortalBlock 7 5 immortalBlock.jpg  immortalBlock 8 5 immortalBlock.jpg  immortalBlock 9 5 immortalBlock.jpg  immortalBlock 10 5 immortalBlock.jpg  immortalBlock 11 5 immortalBlock.jpg  immortalBlock 12 5 immortalBlock.jpg  block 1 6 green.jpg  block 2 6 green.jpg  block 3 6 green.jpg  block 4 6 green.jpg  block 5 6 green.jpg  block 6 6 green.jpg  block 7 6 green.jpg  block 8 6 green.jpg  block 9 6 green.jpg  block 10 6 green.jpg  block 11 6 green.jpg  block 12 6 green.jpg  immortalBlock 0 8 immortalBlock.jpg  immortalBlock 1 8 immortalBlock.jpg  immortalBlock 2 8 immortalBlock.jpg  immortalBlock 3 8 immortalBlock.jpg  immortalBlock 4 8 immortalBlock.jpg  immortalBlock 5 8 immortalBlock.jpg  immortalBlock 6 8 immortalBlock.jpg  immortalBlock 7 8 immortalBlock.jpg  immortalBlock 8 8 immortalBlock.jpg  immortalBlock 9 8 immortalBlock.jpg  immortalBlock 10 8 immortalBlock.jpg  immortalBlock 11 8 immortalBlock.jpg  block 0 9 blue.jpg  block 1 9 blue.jpg  block 2 9 blue.jpg  block 3 9 blue.jpg  block 4 9 blue.jpg  block 5 9 blue.jpg  block 6 9 blue.jpg  block 7 9 blue.jpg  block 8 9 blue.jpg  block 9 9 blue.jpg  block 10 9 blue.jpg  block 11 9 blue.jpg  immortalBlock 1 11 immortalBlock.jpg  immortalBlock 2 11 immortalBlock.jpg  immortalBlock 3 11 immortalBlock.jpg  immortalBlock 4 11 immortalBlock.jpg  immortalBlock 5 11 immortalBlock.jpg  immortalBlock 6 11 immortalBlock.jpg  immortalBlock 7 11 immortalBlock.jpg  immortalBlock 8 11 immortalBlock.jpg  immortalBlock 9 11 immortalBlock.jpg  immortalBlock 10 11 immortalBlock.jpg  immortalBlock 11 11 immortalBlock.jpg  immortalBlock 12 11 immortalBlock.jpg  block 1 12 red.jpg  block 2 12 red.jpg  block 3 12 red.jpg  block 4 12 red.jpg  block 5 12 red.jpg  block 6 12 red.jpg  block 7 12 red.jpg  block 8 12 red.jpg  block 9 12 red.jpg  block 10 12 red.jpg  block 11 12 red.jpg  block 12 12 red.jpg  immortalBlock 0 14 immortalBlock.jpg  immortalBlock 1 14 immortalBlock.jpg  immortalBlock 2 14 immortalBlock.jpg  immortalBlock 3 14 immortalBlock.jpg  immortalBlock 4 14 immortalBlock.jpg  immortalBlock 5 14 immortalBlock.jpg  immortalBlock 6 14 immortalBlock.jpg  immortalBlock 7 14 immortalBlock.jpg  immortalBlock 8 14 immortalBlock.jpg  immortalBlock 9 14 immortalBlock.jpg  immortalBlock 10 14 immortalBlock.jpg  immortalBlock 11 14 immortalBlock.jpg";
        private const string level13 = "immortalBlock 0 0 immortalBlock.jpg  immortalBlock 1 0 immortalBlock.jpg  block 2 0 indigo.jpg  block 3 0 indigo.jpg  block 5 0 green.jpg  block 6 0 red.jpg  block 7 0 green.jpg  block 9 0 indigo.jpg  block 10 0 indigo.jpg  immortalBlock 11 0 immortalBlock.jpg  immortalBlock 12 0 immortalBlock.jpg  immortalBlock 0 1 immortalBlock.jpg  immortalBlock 1 1 immortalBlock.jpg  block 2 1 indigo.jpg  block 3 1 indigo.jpg  block 5 1 green.jpg  block 6 1 red.jpg  block 7 1 green.jpg  block 9 1 indigo.jpg  block 10 1 indigo.jpg  immortalBlock 11 1 immortalBlock.jpg  immortalBlock 12 1 immortalBlock.jpg  block 0 2 indigo.jpg  block 1 2 indigo.jpg  immortalBlock 2 2 immortalBlock.jpg  block 3 2 indigo.jpg  block 5 2 green.jpg  block 6 2 red.jpg  block 7 2 green.jpg  block 9 2 indigo.jpg  immortalBlock 10 2 immortalBlock.jpg  block 11 2 indigo.jpg  block 12 2 indigo.jpg  block 0 3 indigo.jpg  block 1 3 indigo.jpg  block 2 3 indigo.jpg  block 3 3 indigo.jpg wall 39   block 9 3 indigo.jpg  block 10 3 indigo.jpg  block 11 3 indigo.jpg  block 12 3 indigo.jpg  immortalBlock 5 5 immortalBlock.jpg  block 6 5 darkGreen.jpg  immortalBlock 7 5 immortalBlock.jpg  block 5 6 darkGreen.jpg  immortalBlock 6 6 immortalBlock.jpg  block 7 6 darkGreen.jpg  immortalBlock 5 7 immortalBlock.jpg  block 6 7 darkGreen.jpg  immortalBlock 7 7 immortalBlock.jpg  block 0 10 orange.jpg  block 1 10 orange.jpg  block 5 10 violet.jpg  block 6 10 yellow.jpg  block 7 10 violet.jpg  block 11 10 orange.jpg  block 12 10 orange.jpg  block 0 11 orange.jpg  block 1 11 orange.jpg  block 5 11 violet.jpg  block 6 11 yellow.jpg  block 7 11 violet.jpg  block 11 11 orange.jpg  block 12 11 orange.jpg  immortalBlock 0 12 immortalBlock.jpg  immortalBlock 1 12 immortalBlock.jpg  block 5 12 violet.jpg  block 6 12 yellow.jpg  block 7 12 violet.jpg  immortalBlock 11 12 immortalBlock.jpg  immortalBlock 12 12 immortalBlock.jpg  block 0 18 red.jpg  block 1 18 red.jpg  block 2 18 red.jpg  block 3 18 red.jpg  block 4 18 red.jpg  block 5 18 red.jpg  block 6 18 red.jpg  block 7 18 red.jpg  block 8 18 red.jpg  block 9 18 red.jpg  block 10 18 red.jpg  block 11 18 red.jpg  block 12 18 red.jpg";

        private static readonly string[] levels = new string[] { level1, level2, level3, level4, level5, level6, level7, level8, level9, level10, level11, level12, level13 };

        private static IEnumerable<string> splitLevelIntoBlocks(string levelString) {
            Match match = Regex.Match(levelString, @"\s+wall (\d+)\s+");
            Console.WriteLine(Convert.ToChar(byte.Parse(match.Groups[1].Value)));
            levelString = Regex.Replace(levelString, @"\s+wall \d+\s+", "  ");
            return levelString.Split("  ");
        }

        private static BlockInfo parseBlock(string blockString) {
            string[] blockParts = blockString.Split(' ');
            BlockInfo.BlockType blockType = BlockInfo.BlockType.UNKNOWN;
            switch (blockParts[3].Substring(0, blockParts[3].IndexOf('.'))) {
                case "red":
                    blockType = BlockInfo.BlockType.RED;
                    break;
                case "orange":
                    blockType = BlockInfo.BlockType.ORANGE;
                    break;
                case "blue":
                    blockType = BlockInfo.BlockType.BLUE;
                    break;
                case "violet":
                    blockType = BlockInfo.BlockType.VIOLET;
                    break;
                case "indigo":
                    blockType = BlockInfo.BlockType.INDIGO;
                    break;
                case "green":
                    blockType = BlockInfo.BlockType.GREEN;
                    break;
                case "yellow":
                    blockType = BlockInfo.BlockType.YELLOW;
                    break;
                case "grey":
                    blockType = BlockInfo.BlockType.GREY;
                    break;
                case "black":
                    blockType = BlockInfo.BlockType.BLACK;
                    break;
                case "darkGreen":
                    blockType = BlockInfo.BlockType.DARKGREEN;
                    break;
                case "immortalBlock":
                    blockType = BlockInfo.BlockType.IMMORTAL;
                    break;
                default:
                    Console.WriteLine(blockParts[3]);
                    break;
            }
            return new BlockInfo(int.Parse(blockParts[1]), int.Parse(blockParts[2]), blockString.StartsWith("immortal"), blockType);
        }

        static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            foreach (string level in levels) {
                foreach (string block in splitLevelIntoBlocks(level)) {
                    parseBlock(block);
                }
            }
        }
    }
}
