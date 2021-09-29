using System;

namespace SmallWorldEncoding {
    class Program {
        static void Main(string[] args) {
            int[] symbols = new int[13] { 32, 33, 44, 72, 83, 87, 97, 100, 101, 108, 109, 111, 114 };

            // try all factor combinations
            for (int factorA = 9; factorA >= 1; factorA--) {
                for (int factorB = 8; factorB >= 1; factorB--) {
                    // check the whole array
                    foreach (int ascii in symbols) {
                        bool foundValidCouple = false;
                        int multiplierA = 0;
                        while (!foundValidCouple && (multiplierA < 10) && (multiplierA * factorA <= ascii)) {
                            int leftOver = ascii - (multiplierA * factorA);
                            // factorB divides the rest AND the result is a single digit
                            if ((leftOver % factorB == 0) && (leftOver / factorB < 10)) {
                                //foundValidCouple = true;
                                Console.WriteLine(ascii + " can be written as " + factorA + "*" + multiplierA + " plus " + factorB + "*" + (leftOver / factorB));
                            }
                            multiplierA++;
                        }
                    }
                }
            }

            Console.WriteLine("Hello World!");
        }
    }
}
