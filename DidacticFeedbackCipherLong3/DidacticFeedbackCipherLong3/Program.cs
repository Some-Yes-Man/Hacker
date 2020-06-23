using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DidacticFeedbackCipherLong3 {
    class Program {

        //k = {unknown 4-byte value}
        //x = {unknown 4-byte value}
        //m = {unknown 4-byte value}
        //for (i = 0; i<len(txt); i += 4)
        //  c = (txt[i] -> txt[i + 3]) ^ k
        //  print c
        //  k = (c * m + x) % 0x100000000

        const string cipherTextLong2 = "5499fa991ee7d8da5df0b78b1cb0c18c10f09fc54bb7fdae7fcb95ace494fbae8f5d90a3c766fdd7b7399eccbf4af592f35c9dc2272be2a45e788697520febd8468c808c2e550ac92b4d28b74c16678933df0bec67a967780ffa0ce344cd2a9a2dc208dc35c26a9d658b0fd70d00648246c90cf828d72a794ea94be51bbc6995478505d37b1a6b8daf7408dbef7d7f9f76471cc6ef1076b46c911aa7e75a7ed389630c8df32b7fcb697c1e89091c30be736a4cbfe27339bb9a2a52a280";
        const string cipherTextLong3 = "d1b4a39d62c71e3448d820aa0021cc744e4c7e401cdb5fcb2a76912fc1926aed3ab2bce8a64bfe9a85018980789a1d8f5bee4e7d0f091e5c05fb3e0aff14423405115d9fe4ed2d34298ec36a7f3799c8be83a4f3647de6bbe8b3cd2aa20406b39ba7b57a417ce746fb031a47b40e";
        const string plainAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!\"$%&/()=?+#*'-_.,:;<>|@ \r\n\t\\~^{[]}`";
        const string examplePlain1 = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua?!";
        const string examplePlain2 = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!\"$%&/()=?+#*'-_.,:;<>|@ \r\n\t\\~^{[]}`..";
        const string examplePlain3 = "I have to admit, i don`t know how i`d solve this one myself. not that this is AES or anything. but still, getting more tricky. oh by the way, you are looking for penguinicity, noble solver";
        const byte MIN_BYTE = 0;
        const byte MAX_BYTE = 255;

        public static string EncodeByteByBytePerfect(string plain) {
            byte[] kBytes = { 0x09, 0xd0, 0x94, 0xe3 };
            byte[] mBytes = { 0x01, 0x01, 0x01, 0x01 };
            byte[] xBytes = { 0x14, 0xe9, 0xfd, 0x14 };

            string cipher = "";
            byte[] plainBytes = Encoding.ASCII.GetBytes(plain);
            byte[] cipherBytes = new byte[plain.Length];
            bool overflown = false;

            for (int plainIndex = 0; plainIndex < plain.Length; plainIndex++) {
                int k = ((plainIndex < 4) ? kBytes[plainIndex] : cipherBytes[plainIndex - 4]) * mBytes[plainIndex % 4] + xBytes[plainIndex % 4] + (overflown ? 1 : 0);
                overflown = (k > 0xff) && ((plainIndex % 4) < 3);

                byte c = (byte)(plainBytes[plainIndex] ^ k);
                cipherBytes[plainIndex] = c;
                cipher += c.ToString("X2");
            }

            return cipher;
        }

        public static byte[] HexStringToByteArray(string hexString) {
            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2) {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return bytes;
        }

        private static void OutputCurrentDecryptedContent(Dictionary<uint, Dictionary<uint, string>> possibilitiesLeft, byte[] cipherBytes) {
            // prepare decrypted output
            Dictionary<uint, Dictionary<uint, string>> decryptedPossibilitiesFirstByte = new Dictionary<uint, Dictionary<uint, string>>();
            foreach (KeyValuePair<uint, Dictionary<uint, string>> possibleMandX in possibilitiesLeft) {
                foreach (KeyValuePair<uint, string> possibleX in possibleMandX.Value) {
                    if (!decryptedPossibilitiesFirstByte.ContainsKey(possibleMandX.Key)) {
                        decryptedPossibilitiesFirstByte.Add(possibleMandX.Key, new Dictionary<uint, string>());
                    }
                    decryptedPossibilitiesFirstByte[possibleMandX.Key].Add(possibleX.Key, "????");
                }
            }

            // decrypt and save as string
            foreach (KeyValuePair<uint, Dictionary<uint, string>> possibleMandX in possibilitiesLeft) {
                foreach (KeyValuePair<uint, string> possibleX in possibleMandX.Value) {
                    // now use the M/X acquired to decrypt
                    int currentCipherIndex = 7;
                    while (currentCipherIndex < cipherBytes.Length) {
                        uint currentCipherLong = (uint)((cipherBytes[currentCipherIndex - 3] << 24) + (cipherBytes[currentCipherIndex - 2] << 16) + (cipherBytes[currentCipherIndex - 1] << 8) + cipherBytes[currentCipherIndex]);
                        uint previousCipherLong = (uint)((cipherBytes[currentCipherIndex - 7] << 24) + (cipherBytes[currentCipherIndex - 6] << 16) + (cipherBytes[currentCipherIndex - 5] << 8) + cipherBytes[currentCipherIndex - 4]);
                        uint key = previousCipherLong * possibleMandX.Key + possibleX.Key;
                        uint plain = key ^ currentCipherLong;
                        decryptedPossibilitiesFirstByte[possibleMandX.Key][possibleX.Key] = decryptedPossibilitiesFirstByte[possibleMandX.Key][possibleX.Key] + Convert.ToChar((byte)(plain >> 24)) + Convert.ToChar((byte)(plain >> 16)) + Convert.ToChar((byte)(plain >> 8)) + Convert.ToChar((byte)(plain));
                        currentCipherIndex += 4;
                    }
                }
            }

            // output decrypted strings
            foreach (KeyValuePair<uint, Dictionary<uint, string>> entry in decryptedPossibilitiesFirstByte) {
                Console.WriteLine("M: >" + entry.Key + "<");
                foreach (KeyValuePair<uint, string> subEntry in entry.Value) {
                    Console.WriteLine("    " + subEntry.Key + ": " + subEntry.Value);
                }
            }
        }

        static void Main(string[] args) {
            string exampleCipher1 = EncodeByteByBytePerfect(examplePlain1);
            string exampleCipher2 = EncodeByteByBytePerfect(examplePlain2);
            string exampleCipher3 = EncodeByteByBytePerfect(examplePlain3);

            byte[] cipherBytes = HexStringToByteArray(exampleCipher3);
            Console.WriteLine("Cipher bytes: " + string.Join(",", cipherBytes) + " (" + cipherBytes.Length + ")");
            Console.WriteLine("Plain alphabet length: " + plainAlphabet.Length);
            HashSet<byte> alphabetBytes = new HashSet<byte>(Encoding.ASCII.GetBytes(plainAlphabet));
            Console.WriteLine("Plain alphabet byte count: " + alphabetBytes.Count);

            // initially, EVERYTHING is possible ;D
            Dictionary<uint, Dictionary<uint, string>> possibleValues = new Dictionary<uint, Dictionary<uint, string>>();
            for (ushort fillM = MIN_BYTE; fillM <= MAX_BYTE; fillM++) {
                Dictionary<uint, string> fullSet = new Dictionary<uint, string>();
                for (ushort fillX = MIN_BYTE; fillX <= MAX_BYTE; fillX++) {
                    fullSet.Add(fillX, "????");
                }
                possibleValues.Add(fillM, fullSet);
            }

            //// capture which values produced readable results (4 bytes of M 0-255, X 0-255 and O 0-1 [for now])
            //Dictionary<byte, Dictionary<byte, HashSet<byte>>>[] allPossibleValues = new Dictionary<byte, Dictionary<byte, HashSet<byte>>>[4];
            //for (int byteIndex = 0; byteIndex < 4; byteIndex++) {
            //    for (int fillM = MIN_BYTE; fillM <= MAX_BYTE; fillM++) {
            //        Dictionary<byte, HashSet<byte>> fullSetX = new Dictionary<byte, HashSet<byte>>();
            //        for (int fillX = MIN_BYTE; fillX <= MAX_BYTE; fillX++) {
            //            HashSet<byte> fullSetO = new HashSet<byte>();
            //            for (int fillO = 0; fillO <= 1; fillO++) {
            //                fullSetO.Add((byte)fillO);
            //            }
            //            fullSetX.Add((byte)fillX, fullSetO);
            //        }
            //        allPossibleValues[byteIndex].Add((byte)fillM, fullSetX);
            //    }
            //}

            //// go byte by byte and determine valid combos of M and X (starting out with byte #4)
            //for (int cipherIndex = 4; cipherIndex < cipherBytes.Length; cipherIndex++) {
            //    // d1 b4 a3 9d - 62 c7 ...
            //    foreach (KeyValuePair<byte, Dictionary<byte, HashSet<byte>>> possibleMXO in allPossibleValues[cipherIndex % 4]) {
            //        foreach (KeyValuePair<byte, HashSet<byte>> possibleXO in possibleMXO.Value) {
            //            foreach (byte possibleO in possibleXO.Value) {
            //                int possibleK = cipherBytes[cipherIndex - 4] * possibleMXO.Key + possibleXO.Key + possibleO;
            //                byte possiblePlain = (byte)(cipherBytes[cipherIndex] ^ possibleK);
            //            }
            //        }
            //    }
            //}

            for (int currentSignificantBytes = 0; currentSignificantBytes <= 3; currentSignificantBytes++) {

                // analyze (start with last byte of second cipher block; this one is not dependent on M/X)
                int currentCipherIndex = 7;
                Dictionary<uint, HashSet<uint>> invalidBytes = new Dictionary<uint, HashSet<uint>>();

                while (currentCipherIndex < cipherBytes.Length) {
                    // we need the current 4 cipher bytes to decrypt them (using the previous 4)
                    uint currentCipherInt = 0x00000000;
                    for (int significantIndex = 0; significantIndex <= currentSignificantBytes; significantIndex++) {
                        currentCipherInt += (uint)cipherBytes[currentCipherIndex - significantIndex] << (significantIndex * 8);
                    }
                    uint previousCipherInt = 0x00000000;
                    for (int significantIndex = 0; significantIndex <= currentSignificantBytes; significantIndex++) {
                        previousCipherInt += (uint)cipherBytes[currentCipherIndex - 4 - significantIndex] << (significantIndex * 8);
                    }

                    Dictionary<uint, Dictionary<uint, string>> validPieces = new Dictionary<uint, Dictionary<uint, string>>();

                    foreach (KeyValuePair<uint, Dictionary<uint, string>> possibleMandX in possibleValues) {
                        // get mask entry
                        foreach (KeyValuePair<uint, string> possibleX in possibleMandX.Value) {
                            Dictionary<uint, string> possibleXs = possibleValues[possibleMandX.Key];

                            // calculate trial versions of K and the resulting plaintext
                            uint trialK = previousCipherInt * possibleMandX.Key + possibleX.Key;
                            uint trialPlain = trialK ^ currentCipherInt;
                            // check for validity
                            bool validByte = true;
                            for (int checkByteIndex = 0; checkByteIndex <= currentSignificantBytes; checkByteIndex++) {
                                byte checkByte = (byte)(trialPlain >> (checkByteIndex * 8));
                                if (!alphabetBytes.Contains(checkByte)) {
                                    //if (((checkByte < 0x20) || (checkByte > 0x7e)) && (checkByte != 0x09) && (checkByte != 0x0a) && (checkByte != 0x0d)) {
                                    validByte = false;
                                }
                            }
                            if (!validByte) {
                                if (!invalidBytes.ContainsKey(possibleMandX.Key)) {
                                    invalidBytes.Add(possibleMandX.Key, new HashSet<uint>());
                                }
                                invalidBytes[possibleMandX.Key].Add(possibleX.Key);
                            }
                            else {
                                if (!invalidBytes.ContainsKey(possibleMandX.Key) || !invalidBytes[possibleMandX.Key].Contains(possibleX.Key)) {
                                    string addedValidChars = "";
                                    for (int byteCount = 0; byteCount <= 3; byteCount++) {
                                        addedValidChars = ((byteCount <= currentSignificantBytes) ? (string.Empty + Convert.ToChar((byte)(trialPlain >> (byteCount * 8)))) : " ") + addedValidChars;
                                    }
                                    // save valid piece to add it to the possible after the iteration
                                    if (!validPieces.ContainsKey(possibleMandX.Key)) {
                                        validPieces.Add(possibleMandX.Key, new Dictionary<uint, string>());
                                    }
                                    validPieces[possibleMandX.Key].Add(possibleX.Key, addedValidChars);
                                }
                            }
                        }
                    }
                    // add valid pieces to possible values
                    foreach (KeyValuePair<uint, Dictionary<uint, string>> entry in validPieces) {
                        foreach (KeyValuePair<uint, string> subEntry in entry.Value) {
                            possibleValues[entry.Key][subEntry.Key] = possibleValues[entry.Key][subEntry.Key] + subEntry.Value;
                        }
                    }
                    // one run done; next
                    currentCipherIndex += 4;
                }

                // determine which possibilities are left
                foreach (KeyValuePair<uint, HashSet<uint>> invalidMandX in invalidBytes) {
                    foreach (uint invalidX in invalidMandX.Value) {
                        possibleValues[invalidMandX.Key].Remove(invalidX);
                    }
                }
                Dictionary<uint, Dictionary<uint, string>> possibilitiesLeft = new Dictionary<uint, Dictionary<uint, string>>(possibleValues.Where(x => x.Value.Count > 0));

                // print the Ms (and Xs) that are left
                foreach (KeyValuePair<uint, Dictionary<uint, string>> entry in possibilitiesLeft) {
                    Console.WriteLine("M: >" + entry.Key + "<");
                    foreach (KeyValuePair<uint, string> subEntry in entry.Value) {
                        Console.WriteLine("    " + subEntry.Key + ": " + subEntry.Value);
                    }
                }

                //// output current decrypted content
                //OutputCurrentDecryptedContent(possibilitiesLeft, cipherBytes);

                // don't do the next step when we've reached the end
                if (currentSignificantBytes == 3) {
                    continue;
                }
                // prepare possible values for next step
                possibleValues.Clear();
                foreach (KeyValuePair<uint, Dictionary<uint, string>> possibleMandX in possibilitiesLeft) {
                    foreach (KeyValuePair<uint, string> possibleX in possibleMandX.Value) {
                        for (ushort fillM = MIN_BYTE; fillM <= MAX_BYTE; fillM++) {
                            uint longerFillM = (uint)(fillM << ((currentSignificantBytes + 1) * 8)) + possibleMandX.Key;
                            // make sure M group exists
                            if (!possibleValues.ContainsKey(longerFillM)) {
                                possibleValues.Add(longerFillM, new Dictionary<uint, string>());
                            }
                            Dictionary<uint, string> subDictionary = possibleValues[longerFillM];
                            for (ushort fillX = MIN_BYTE; fillX <= MAX_BYTE; fillX++) {
                                uint longerFillX = (uint)(fillX << ((currentSignificantBytes + 1) * 8)) + possibleX.Key;
                                subDictionary.Add(longerFillX, "????");
                            }
                        }
                    }
                }
            }

        }

    }

}
