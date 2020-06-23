using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;

namespace PrimalPi {
    public class Primality {

        private Random random = new Random();
        private RandomNumberGenerator rng = RandomNumberGenerator.Create();

        public Primality() {

        }

        public enum NumberType {
            Composite,
            Prime
        }

        public bool IsPrimeMillerRabin(BigInteger integer) {
            NumberType type = MillerRabin(integer, 5);
            return type == NumberType.Prime;
        }

        public bool IsPrimePseudo(BigInteger integer) {
            NumberType type = PseudoPrime(integer);
            return type == NumberType.Prime;
        }

        // Primality testing based on Miller-Rabin
        public NumberType MillerRabin(BigInteger n, int s) {
            BigInteger nMinusOne = BigInteger.Subtract(n, 1);

            for (int j = 1; j <= s; j++) {
                BigInteger a = RandomInRange(this.rng, 1, nMinusOne);

                if (Witness(a, n)) {
                    return NumberType.Composite;
                }
            }

            return NumberType.Prime;
        }

        public static BigInteger RandomInRange(RandomNumberGenerator rng, BigInteger min, BigInteger max) {
            if (min > max) {
                var buff = min;
                min = max;
                max = buff;
            }

            // offset to set min = 0
            BigInteger offset = -min;
            min = 0;
            max += offset;

            var value = randomInRangeFromZeroToPositive(rng, max) - offset;
            return value;
        }

        private static BigInteger randomInRangeFromZeroToPositive(RandomNumberGenerator rng, BigInteger max) {
            BigInteger value;
            var bytes = max.ToByteArray();

            // count how many bits of the most significant byte are 0
            // NOTE: sign bit is always 0 because `max` must always be positive
            byte zeroBitsMask = 0x00;

            var mostSignificantByte = bytes[bytes.Length - 1];

            // we try to set to 0 as many bits as there are in the most significant byte, starting from the left (most significant bits first)
            // NOTE: `i` starts from 7 because the sign bit is always 0
            for (var i = 7; i >= 0; i--) {
                // we keep iterating until we find the most significant non-0 bit
                if ((mostSignificantByte & (0x01 << i)) != 0) {
                    var zeroBits = 7 - i;
                    zeroBitsMask = (byte)(0xff >> zeroBits);
                    break;
                }
            }

            do {
                rng.GetBytes(bytes);

                // set most significant bits to 0 (because `value > max` if any of these bits is 1)
                bytes[bytes.Length - 1] &= zeroBitsMask;

                value = new BigInteger(bytes);

                // `value > max` 50% of the times, in which case the fastest way to keep the distribution uniform is to try again
            } while (value > max);

            return value;
        }

        // Generates a random BigInteger between min and max
        public BigInteger Random(BigInteger min, BigInteger max) {
            byte[] maxBytes = max.ToByteArray();
            BitArray maxBits = new BitArray(maxBytes);

            for (int i = 0; i < maxBits.Length; i++) {
                // Randomly set the bit
                int randomInt = this.random.Next();
                if ((randomInt % 2) == 0) {
                    // Reverse the bit
                    maxBits[i] = !maxBits[i];
                }
            }

            BigInteger result = new BigInteger();

            // Convert the bits back to a BigInteger
            for (int k = (maxBits.Count - 1); k >= 0; k--) {
                BigInteger bitValue = 0;

                if (maxBits[k]) {
                    bitValue = BigInteger.Pow(2, k);
                }

                result = BigInteger.Add(result, bitValue);
            }

            // Generate the random number
            BigInteger randomBigInt = BigInteger.ModPow(result, 1, BigInteger.Add(max, min));
            return randomBigInt;
        }

        // Pseudo primality testing with Fermat's theorem
        public NumberType PseudoPrime(BigInteger n) {
            BigInteger modularExponentiation = ModularExponentiation(2, BigInteger.Subtract(n, 1), n);
            if (!modularExponentiation.IsOne) {
                return NumberType.Composite;
            }
            else {
                return NumberType.Prime;
            }
        }

        public bool Witness(BigInteger a, BigInteger n) {
            KeyValuePair<int, BigInteger> tAndU = GetTAndU(BigInteger.Subtract(n, 1));
            int t = tAndU.Key;
            BigInteger u = tAndU.Value;
            BigInteger[] x = new BigInteger[t + 1];

            x[0] = ModularExponentiation(a, u, n);

            for (int i = 1; i <= t; i++) {
                // x[i] = x[i-1]^2 mod n
                x[i] = BigInteger.ModPow(BigInteger.Multiply(x[i - 1], x[i - 1]), 1, n);
                BigInteger minus = BigInteger.Subtract(x[i - 1], BigInteger.Subtract(n, 1));

                if (x[i] == 1 && x[i - 1] != 1 && !minus.IsZero) {
                    return true;
                }
            }

            if (!x[t].IsOne) {
                return true;
            }

            return false;
        }

        public KeyValuePair<int, BigInteger> GetTAndU(BigInteger nMinusOne) {
            // Convert n - 1 to a byte array
            byte[] nBytes = nMinusOne.ToByteArray();
            BitArray bits = new BitArray(nBytes);
            int t = 0;
            BigInteger u = new BigInteger();

            int n = bits.Count - 1;
            bool lastBit = bits[n];

            // Calculate t
            while (!lastBit) {
                t++;
                n--;
                lastBit = bits[n];
            }

            for (int k = ((bits.Count - 1) - t); k >= 0; k--) {
                BigInteger bitValue = 0;

                if (bits[k]) {
                    bitValue = BigInteger.Pow(2, k);
                }

                u = BigInteger.Add(u, bitValue);
            }

            KeyValuePair<int, BigInteger> tAndU = new KeyValuePair<int, BigInteger>(t, u);
            return tAndU;
        }

        public BigInteger ModularExponentiation(BigInteger a, BigInteger b, BigInteger n) {
            // a^b mod n
            return BigInteger.ModPow(a, b, n);
        }
    }
}
