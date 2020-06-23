using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace LazyMaze {

    class Program {

        class Coords {
            public int X;
            public int Y;

            public Coords(int x, int y) {
                this.X = x;
                this.Y = y;
            }
        }

        class MazeSolver {
            static bool DEBUG = false;
            static int dimX = 101;
            static int dimY = 101;
            static string requestUrl = "http://www.hacker.org/challenge/misc/maze.php?steps=";
            // init maze (all unknown)
            // >1 : path
            enum MAZE { START = 1, UNKNOWN = 0, BLOCKED = -1, EDGE = -2, POSSIBLE = 2, END = -3 };
            enum DIRECTIONS { UP, DOWN, LEFT, RIGHT };

            int[,] maze = new int[dimX, dimY];
            Coords startingCoords = new Coords((int)(dimX / 2), 1);

            public MazeSolver() {
                for (int i = 0; i < dimX; i++) {
                    for (int j = 0; j < dimY; j++) {
                        maze[ i, j ] = (int) MAZE.UNKNOWN;
                    }
                }
                maze[ startingCoords.X, startingCoords.Y ] = (int) MAZE.START;
            }

            public string GetCurrentPathString(Coords coords) {
                Coords currentTracePosition = new Coords(coords.X, coords.Y);
                int step = maze[currentTracePosition.X, currentTracePosition.Y];
                bool pathEnded = false;
                string retString = string.Empty;

                while (!pathEnded && (step > 1)) {
                    bool nextStepFound = false;
                    // UP
                    if (!nextStepFound && (currentTracePosition.Y > 0) && (maze[currentTracePosition.X, currentTracePosition.Y - 1] == (step - 1))) {
                        nextStepFound = true;
                        step--;
                        currentTracePosition.Y--;
                        retString = "D" + retString;
                    }
                    // DOWN
                    if (!nextStepFound && (currentTracePosition.Y < (dimY - 1)) && (maze[currentTracePosition.X, currentTracePosition.Y + 1] == (step - 1))) {
                        nextStepFound = true;
                        step--;
                        currentTracePosition.Y++;
                        retString = "U" + retString;
                    }
                    // LEFT
                    if (!nextStepFound && (currentTracePosition.X > 0) && (maze[currentTracePosition.X - 1, currentTracePosition.Y] == (step - 1))) {
                        nextStepFound = true;
                        step--;
                        currentTracePosition.X--;
                        retString = "R" + retString;
                    }
                    // RIGHT
                    if (!nextStepFound && (currentTracePosition.X < (dimX - 1)) && (maze[currentTracePosition.X + 1, currentTracePosition.Y] == (step - 1))) {
                        nextStepFound = true;
                        step--;
                        currentTracePosition.X++;
                        retString = "L" + retString;
                    }
                    // found anything?
                    if (!nextStepFound) {
                        pathEnded = true;
                    }
                    nextStepFound = false;
                }

                return retString;
            }

            MAZE TryStep(Coords coords, DIRECTIONS direction) {
                string responseString = string.Empty;
                string requestString = GetCurrentPathString(coords);

                switch (direction) {
                    case DIRECTIONS.UP:
                        requestString += "U";
                        break;
                    case DIRECTIONS.DOWN:
                        requestString += "D";
                        break;
                    case DIRECTIONS.LEFT:
                        requestString += "L";
                        break;
                    case DIRECTIONS.RIGHT:
                        requestString += "R";
                        break;
                }

                HttpWebRequest webRequest = WebRequest.CreateHttp(requestUrl + requestString);
                using (WebResponse webResponse = webRequest.GetResponse()) {
                    using (Stream responseStream = webResponse.GetResponseStream()) {
                        using (StreamReader reader = new StreamReader(responseStream)) {
                            responseString = reader.ReadToEnd();
                            if (DEBUG) { Console.Write(requestString + " "); } else { Console.Write("#"); }
                        }
                    }
                }

                if (Regex.IsMatch(responseString, "keep moving")) {
                    return MAZE.POSSIBLE;
                }
                if (Regex.IsMatch(responseString, "boom")) {
                    return MAZE.BLOCKED;
                }
                if (Regex.IsMatch(responseString, "off the edge of the world")) {
                    return MAZE.EDGE;
                }
                Console.WriteLine(responseString);
                return MAZE.END;
            }

            public void TakeStep() {
                TakeStep(startingCoords, (int)MAZE.START);
            }

            public void TakeStep(Coords coords, int step) {
                for (int i = 0; i < 4; i++) {
                    // UP
                    if ((coords.Y > 0) && (maze[coords.X, coords.Y - 1] == (int)MAZE.UNKNOWN)) {
                        if (DEBUG) { Console.Write("Trying UP from " + coords.X + ":" + coords.Y + " "); }
                        MAZE foo = TryStep(coords, DIRECTIONS.UP);
                        switch (foo) {
                            case MAZE.BLOCKED:
                                if (DEBUG) { Console.WriteLine("BLOCKED"); }
                                maze[coords.X, coords.Y - 1] = (int)MAZE.BLOCKED;
                                break;
                            case MAZE.EDGE:
                                if (DEBUG) { Console.WriteLine("EDGE"); }
                                maze[coords.X, coords.Y - 1] = (int)MAZE.EDGE;
                                break;
                            case MAZE.END:
                                if (DEBUG) { Console.WriteLine("???"); }
                                Console.WriteLine();
                                Console.WriteLine(GetCurrentPathString(coords));
                                break;
                            case MAZE.POSSIBLE:
                                if (DEBUG) { Console.WriteLine("SUCCESS"); }
                                maze[coords.X, coords.Y - 1] = step + 1;
                                TakeStep(new Coords(coords.X, coords.Y - 1), step + 1);
                                break;
                        }
                    }
                    // DOWN
                    if ((coords.Y < (dimY - 1)) && (maze[coords.X, coords.Y + 1] == (int)MAZE.UNKNOWN)) {
                        if (DEBUG) { Console.Write("Trying DOWN from " + coords.X + ":" + coords.Y + " "); }
                        MAZE foo = TryStep(coords, DIRECTIONS.DOWN);
                        switch (foo) {
                            case MAZE.BLOCKED:
                                if (DEBUG) { Console.WriteLine("BLOCKED"); }
                                maze[coords.X, coords.Y + 1] = (int)MAZE.BLOCKED;
                                break;
                            case MAZE.EDGE:
                                if (DEBUG) { Console.WriteLine("EDGE"); }
                                maze[coords.X, coords.Y + 1] = (int)MAZE.EDGE;
                                break;
                            case MAZE.END:
                                if (DEBUG) { Console.WriteLine("???"); }
                                Console.WriteLine();
                                Console.WriteLine(GetCurrentPathString(coords));
                                break;
                            case MAZE.POSSIBLE:
                                if (DEBUG) { Console.WriteLine("SUCCESS"); }
                                maze[coords.X, coords.Y + 1] = step + 1;
                                TakeStep(new Coords(coords.X, coords.Y + 1), step + 1);
                                break;
                        }
                    }
                    // LEFT
                    if ((coords.X > 0) && (maze[coords.X - 1, coords.Y] == (int)MAZE.UNKNOWN)) {
                        if (DEBUG) { Console.Write("Trying LEFT from " + coords.X + ":" + coords.Y + " "); }
                        MAZE foo = TryStep(coords, DIRECTIONS.LEFT);
                        switch (foo) {
                            case MAZE.BLOCKED:
                                if (DEBUG) { Console.WriteLine("BLOCKED"); }
                                maze[coords.X - 1, coords.Y] = (int)MAZE.BLOCKED;
                                break;
                            case MAZE.EDGE:
                                if (DEBUG) { Console.WriteLine("EDGE"); }
                                maze[coords.X - 1, coords.Y] = (int)MAZE.EDGE;
                                break;
                            case MAZE.END:
                                if (DEBUG) { Console.WriteLine("???"); }
                                Console.WriteLine();
                                Console.WriteLine(GetCurrentPathString(coords));
                                break;
                            case MAZE.POSSIBLE:
                                if (DEBUG) { Console.WriteLine("SUCCESS"); }
                                maze[coords.X - 1, coords.Y] = step + 1;
                                TakeStep(new Coords(coords.X - 1, coords.Y), step + 1);
                                break;
                        }
                    }
                    // RIGHT
                    if ((coords.X < (dimX - 1)) && (maze[coords.X + 1, coords.Y] == (int)MAZE.UNKNOWN)) {
                        if (DEBUG) { Console.Write("Trying RIGHT from " + coords.X + ":" + coords.Y + " "); }
                        MAZE foo = TryStep(coords, DIRECTIONS.RIGHT);
                        switch (foo) {
                            case MAZE.BLOCKED:
                                if (DEBUG) { Console.WriteLine("BLOCKED"); }
                                maze[coords.X + 1, coords.Y] = (int)MAZE.BLOCKED;
                                break;
                            case MAZE.EDGE:
                                if (DEBUG) { Console.WriteLine("EDGE"); }
                                maze[coords.X + 1, coords.Y] = (int)MAZE.EDGE;
                                break;
                            case MAZE.END:
                                if (DEBUG) { Console.WriteLine("???"); }
                                Console.WriteLine();
                                Console.WriteLine(GetCurrentPathString(coords));
                                break;
                            case MAZE.POSSIBLE:
                                if (DEBUG) { Console.WriteLine("SUCCESS"); }
                                maze[coords.X + 1, coords.Y] = step + 1;
                                TakeStep(new Coords(coords.X + 1, coords.Y), step + 1);
                                break;
                        }
                    }
                }
                //PrintMaze();
            }

            public void PrintMaze() {
                for (int y = 0; y < dimY; y++) {
                    for (int x = 0; x < dimX; x++) {
                        Console.Write(maze[x, y].ToString().PadLeft(2) + " ");
                    }
                    Console.WriteLine();
                }
            }
        }

        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            Console.ReadKey(true);

            MazeSolver solver = new MazeSolver();
            solver.TakeStep();
            //solver.PrintMaze();
            //Console.WriteLine(solver.GetCurrentPathString());
            Console.ReadKey(true);
        }
    }
}
