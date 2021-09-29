using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SnakeArithmetics {
    class Program {

        private static BigInteger Dick(BigInteger n) {
            BigInteger tmp = (2 * n) - 1;
            if (n == 0) {
                //Console.WriteLine("Dick: end");
                return 1;
            }
            else {
                //Console.WriteLine("Dick: " + tmp);
                return tmp * Dick(n - 1);
            }
        }

        private static BigInteger Nuts(BigInteger n) {
            BigInteger tmp = 4 * n * n - 8 * n + 3;
            if (n == 0) {
                //Console.WriteLine("Nuts: end");
                return 1;
            }
            else {
                //Console.WriteLine("Nuts: " + tmp);
                return tmp * Nuts(n - 2) + 7 * Dick(n - 2);
            }
        }

        static void Main(string[] args) {
            for (int i = 0; i < 100; i++) {
                Console.WriteLine(i + " : " + Nuts(4 * i) / Dick(4 * i) * i);
            }

            BigInteger oneB = BigInteger.Parse("1000000000000");
            BigInteger result = 1;
            for (long i = 0; i < 110000000; i += 2) {
                BigInteger foo = (4 * i * i) + (8 * i) + 3;
                result += BigInteger.Divide(BigInteger.Parse("7000000000000"), foo);
                if (i % 10000000 == 0) {
                    Console.WriteLine(i + " : " + (result + oneB));
                    //1000000000000
                    //3748893563459
                }
            }
            Console.ReadKey(true);
        }
    }
}
