using System;
using System.Text;

namespace Feedback1 {
    class Program {
        private static string challengeText = "751a6f1d3d5c3241365321016c05620a7e5e34413246660461412e5a2e412c49254a24";

        private static string encrypt(string plainText, byte key) {
            byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);
            string cipherText = string.Empty;
            byte cipherByte = 0;
            byte currentKey = key;
            for (int i = 0; i < plainBytes.Length; i++) {
                cipherByte = (byte)(plainBytes[i] ^ currentKey);
                cipherText += cipherByte.ToString("X2");
                currentKey = cipherByte;
            }
            return cipherText;
        }

        private static string decrypt(string cipherText, byte key) {
            // abort if cipher text is odd
            if ((cipherText.Length % 2) != 0) {
                return "Odd cipher text length... aborting.";
            }
            byte[] cipherBytes = new byte[cipherText.Length / 2];
            for (int i = 0; i < cipherBytes.Length; i++) {
                cipherBytes[i] = Convert.ToByte(cipherText.Substring(i * 2, 2), 16);
            }
            byte[] plainBytes = new byte[cipherBytes.Length];
            byte currentKey = key;
            for (int i = 0; i < cipherBytes.Length; i++) {
                plainBytes[i] = (byte)(cipherBytes[i] ^ currentKey);
                currentKey = cipherBytes[i];
            }
            return Encoding.ASCII.GetString(plainBytes);
        }

        static void Main(string[] args) {
            // test
            byte key = 0x12;
            string cipher = encrypt("This is a secret message!", key);
            Console.WriteLine(cipher);
            Console.WriteLine(decrypt(cipher, key));
            Console.ReadKey(true);
            return;

            for (int mayBeKey = 0; mayBeKey < 256; mayBeKey++) {
                Console.WriteLine(mayBeKey.ToString("X2") + " : " + decrypt(challengeText, (byte)mayBeKey));
            }
            Console.ReadKey(true);
        }
    }
}
