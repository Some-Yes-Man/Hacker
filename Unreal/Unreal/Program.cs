using System;
using System.Collections;
using System.Numerics;
using System.Text;

namespace Unreal {

    class Program {

        public static byte[] ToByteArray(BitArray bits) {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0)
                numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++) {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));

                bitIndex++;
                if (bitIndex == 8) {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        static void Main(string[] args) {
            BigInteger currentRemainingDigits = BigInteger.Parse("32971794167081629837167802936780735564829276610423837302600844921076244175368942281062484902431623324606319812044624953502085251067067529255781258678513094235031944233170753846002071232563262824414225592308225272047214587483996563113189135261709407232039814428784434529932459647536748811690069379897038318604072938986788088212224388255707253918843988663750392635832401316929346152919657302073099893616913118698764266886236278890924512661722521352402602020028996390203930569587652348815252346435420262558798398950859424418792607365325428010635063191231970046025622612322668833438328410412321289868486928753554821014404296875");
            BigInteger currentFraction = BigInteger.Parse("50000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");

            int divCount = 0;
            BigInteger remainder = 0;
            BitArray bitStream = new BitArray(700);

            // get bitstream
            while (currentRemainingDigits > 0) {
                if (currentRemainingDigits < currentFraction) {
                    //Console.Write("0");
                    bitStream[divCount] = false;
                }
                else {
                    currentRemainingDigits -= currentFraction;
                    //Console.Write("1");
                    bitStream[divCount] = true;
                }
                currentFraction = BigInteger.DivRem(currentFraction, 2, out remainder);
                if (!remainder.IsZero && !currentRemainingDigits.IsZero) {
                    Console.WriteLine("Division problem during step #" + divCount);
                    Console.ReadKey(true);
                    return;
                }
                Console.WriteLine(currentRemainingDigits);
                divCount++;
            }
            //bitStream[ divCount ] = false;
            Console.WriteLine();
            Console.WriteLine("Division Count: " + divCount);
            byte[] byteStream = ToByteArray(bitStream);
            //for (int i = 288; i <= divCount; i++) {
            //    Console.Write(bitStream[ i ] ? "1" : "0");
            //}
            Console.WriteLine();
            BigInteger multiplier = 1;
            BigInteger sum = 0;
            for (int i = divCount - 1; i >= 288; i--) {
                sum += bitStream[i] ? multiplier : 0;
                multiplier *= 2;
            }
            //for (int i = 288; i <= divCount; i++) {
            //    sum += (!bitStream[ i ] ? multiplier : 0);
            //    multiplier *= 2;
            //}
            Console.WriteLine(sum);
            Console.WriteLine();

            BigInteger root = BigInteger.Parse("1000");
            Console.WriteLine("First estimation:");
            for (int i = 0; i < 500; i++) {
                root = ((sum / root / root) + root + root) / 3;
                Console.Write("#");
            }

            Console.WriteLine();
            Console.WriteLine("nRoot: " + sum);
            Console.WriteLine("yRoot: " + (root * root * root));

            bool countUp = root * root * root < sum;
            BigInteger stepSize = BigInteger.Parse("100000000000000000000000");
            BigInteger oldRoot = BigInteger.One;
            while (oldRoot != root) {
                oldRoot = root;
                root += (countUp ? 1 : -1) * stepSize;
                if ((countUp && (root * root * root > sum)) || (!countUp && (root * root * root < sum))) {
                    Console.WriteLine("Direction changed.");
                    countUp = !countUp;
                    stepSize /= 10;
                }
                Console.WriteLine("Improved: [s:" + stepSize + " r:" + root + "]");
                Console.WriteLine("nRoot: " + sum);
                Console.WriteLine("yRoot: " + (root * root * root));
                //Console.ReadKey(true);
            }

            String anotherCastle = Encoding.ASCII.GetString(byteStream);
            foreach (byte b in byteStream) {
                Console.Write("#" + b);
            }
            Console.WriteLine();
            Console.WriteLine(anotherCastle);
            String again = anotherCastle.Substring(anotherCastle.IndexOf("`") + 1, anotherCastle.LastIndexOf("'") - anotherCastle.IndexOf("`") - 1);
            Console.WriteLine(anotherCastle.IndexOf("`"));
            Console.WriteLine(anotherCastle.LastIndexOf("'") - anotherCastle.IndexOf("`") - 1);
            Console.WriteLine(again);
            Console.ReadKey(true);
        }
    }
}
