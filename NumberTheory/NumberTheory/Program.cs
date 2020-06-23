using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NumberTheory {
    class Program {
        static void Main(string[] args) {
            BigInteger power = 1;
            BigInteger data = BigInteger.Parse("36484379009457399269217182889395826722660566693989257289404709863891849615322840169192133464099837107563290320068627859223102364122264401785848633686914239718396824942863542362872670850647423969609315959515511402019435615717737240510626468808851903266920099765545245394707");
            Console.WriteLine(BigInteger.Remainder(data, 3));
            for (int i = 2; i <= 903; i++) {
                power *= 2;
            }
            Console.WriteLine("Data  length: " + data.ToString().Length + " ( " + data.ToString().Substring(0, 10) + " )");
            Console.WriteLine("Power length: " + power.ToString().Length + " ( " + power.ToString().Substring(0, 10) + " )");
            Console.ReadKey(true);
            BitArray bits = new BitArray(903);
            for (int i = 903; i >= 1; i--) {
                if (power <= data) {
                    bits[ i - 1 ] = true;
                    data -= power;
                } else {
                    bits[ i - 1 ] = false;
                }
                power /= 2;
            }
            Console.WriteLine("Bits calculated.");
            Console.ReadKey(true);

        }
    }
}
