using System;
using System.Text;
using System.Text.RegularExpressions;

namespace FeedbackLong2 {
    class Program {
        private static string challengeText = "5499fa991ee7d8da5df0b78b1cb0c18c10f09fc54bb7fdae7fcb95ace494fbae8f5d90a3c766fdd7b7399eccbf4af592f35c9dc2272be2a45e788697520febd8468c808c2e550ac92b4d28b74c16678933df0bec67a967780ffa0ce344cd2a9a2dc208dc35c26a9d658b0fd70d00648246c90cf828d72a794ea94be51bbc6995478505d37b1a6b8daf7408dbef7d7f9f76471cc6ef1076b46c911aa7e75a7ed389630c8df32b7fcb697c1e89091c30be736a4cbfe27339bb9a2a52a280";

        private static string encrypt(string plainText, int key, int keyModifier) {
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
                currentKey = (int)(((long)cipherInt + keyModifier) % 0x100000000);
            }
            return cipherText;
        }

        private static string decrypt(string cipherText, int key, int keyModifier) {
            // abort if cipher text is odd
            if ((cipherText.Length % 8) != 0) {
                //Console.WriteLine("Odd cipher text length...");
                cipherText = cipherText.Substring(0, (cipherText.Length / 8) * 8);
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
                currentKey = (int)(((long)cipherInts[i] + keyModifier) % 0x100000000);
            }
            return Encoding.ASCII.GetString(plainBytes);
        }

        static void Main(string[] args) {
            //// test
            //int key = 0x12345678;
            //int modifier = 0x21436587;
            //string cipher = encrypt("This is a secret message!", key, modifier);
            //Console.WriteLine(cipher);
            //Console.WriteLine(decrypt(cipher, key, modifier));
            //Console.ReadKey(true);
            //return;

            // "^[a-zA-Z0-9,! '#%&-:;<>=@_Æ]+$"
            Regex validation = new Regex("^(...[a-zA-Z0-9,! '#%&-:;<>=@_])+$", RegexOptions.Compiled);
            for (int x1 = 0; x1 <= 255; x1++) {
                for (int x2 = 232; x2 <= 232; x2++) {
                    for (int x3 = 254; x3 <= 254; x3++) {
                        for (int x4 = 21; x4 <= 21; x4++) {
                            string possiblePlain = decrypt(challengeText, 0x00000000, (int)((x1 << 24) | (x2 << 16) | (x3 << 8) | x4));
                            Console.Write(x1 + ":" + x2 + ":" + x3 + ":" + x4 + " - ");
                            for (int i = 0; i < possiblePlain.Length; i++) {
                                if (i % 4 == 0) {
                                    Console.Write(possiblePlain.Substring(i, 1));
                                }
                                if (i % 4 == 1) {
                                    Console.Write(possiblePlain.Substring(i, 1));
                                }
                                if (i % 4 == 2) {
                                    Console.Write(possiblePlain.Substring(i, 1));
                                }
                                if (i % 4 == 3) {
                                    Console.Write(possiblePlain.Substring(i, 1));
                                }
                            }
                            Console.WriteLine();
                            if (validation.IsMatch(possiblePlain, 4)) {
                                Console.WriteLine(possiblePlain);
                            }
                        }
                    }
                }
            }
            Console.ReadKey(true);
        }
    }
}
