using System;
using System.Text;

namespace Feedback1Long {
    class Program {
        private static string challengeText = "e5534adac53023aaad55518ac42671f8a1471d94d8676ce1b11309c1c27a64b1ae1f4a91c73f2bfce74c5e8e826c27e1f74c4f8081296ff3ee4519968a6570e2aa0709c2c4687eece44a1589903e79ece75117cec73864eebe57119c9e367fefe9530dc1";

        private static string encrypt(string plainText, int key) {
            while ((plainText.Length % 4) != 0) {
                plainText += " ";
            }
            byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);
            string cipherText = string.Empty;
            int cipherInt = 0;
            int currentKey = key;
            for (int i = 0; i < plainBytes.Length; i += 4) {
                cipherInt = (int)(((plainBytes[i] << 24) | (plainBytes[i + 1] << 16) | (plainBytes[i + 2] << 8) | (plainBytes[i + 3])) ^ currentKey);
                cipherText += cipherInt.ToString("X8");
                currentKey = cipherInt;
            }
            return cipherText;
        }

        private static string decrypt(string cipherText, int key) {
            // abort if cipher text is odd
            if ((cipherText.Length % 8) != 0) {
                return "Odd cipher text length... aborting.";
            }
            Int32[] cipherInts = new Int32[cipherText.Length / 8];
            for (int i = 0; i < cipherInts.Length; i++) {
                cipherInts[i] = Convert.ToInt32(cipherText.Substring(i * 8, 8), 16);
            }
            byte[] plainBytes = new byte[cipherText.Length];
            int currentKey = key;
            for (int i = 0; i < cipherInts.Length; i++) {
                int tmpPlain = cipherInts[i] ^ currentKey;
                plainBytes[i * 4] = (byte)((tmpPlain >> 24) & 0xff);
                plainBytes[i * 4 + 1] = (byte)((tmpPlain >> 16) & 0xff);
                plainBytes[i * 4 + 2] = (byte)((tmpPlain >> 8) & 0xff);
                plainBytes[i * 4 + 3] = (byte)(tmpPlain & 0xff);
                currentKey = cipherInts[i];
            }
            return Encoding.ASCII.GetString(plainBytes);
        }

        static void Main(string[] args) {
            // test
            int key = 0x12345678;
            string cipher = encrypt("This is a secret message!", key);
            Console.WriteLine(cipher);
            Console.WriteLine(decrypt(cipher, key));
            Console.ReadKey(true);
            return;

            Console.WriteLine(decrypt(challengeText, 0x00000000));
            Console.ReadKey(true);
        }
    }
}
