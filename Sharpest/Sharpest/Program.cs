using System;

namespace Sharpest {
    class Program {
        private static void Main(string[] args) {
            Console.WriteLine("calculating...");
            //int num = 99;
            //for (int index = num; index >= 0; --index) {
            //    Console.WriteLine(index);
            //    Console.WriteLine("val: " + Program.calc(num - index).ToString());
            //}
            Console.WriteLine("val: " + Program.calc(99).ToString());
            Console.ReadKey(true);
            Console.ReadKey(true);
            Console.ReadKey(true);
        }

        private static int calc(int num) {
            int num1 = 0;
            for (int index1 = 0; index1 < num; ++index1) {
                int length1 = ((index1 > 9) ? 2 : 1);
                for (int index2 = 0; index2 < num; ++index2) {
                    int length2 = ((index2 > 9) ? 2 : 1);
                    for (int index3 = 0; index3 < num; ++index3) {
                        int length3 = ((index3 > 9) ? 2 : 1);
                        for (int index4 = 0; index4 < num; ++index4) {
                            int length4 = ((index4 > 9) ? 2 : 1);
                            for (int index5 = 0; index5 < num; ++index5) {
                                num1 += length1 + length2 + length3 + length4 + ((index5 > 9) ? 2 : 1) + 16;
                            }
                        }
                    }
                }
            }
            return num1;
        }
    }
}
