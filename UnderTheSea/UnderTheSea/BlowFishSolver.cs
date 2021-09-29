using Elskom.Generic.Libs;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.IO;
using System.Text;

namespace UnderTheSea {

    public class BlowFishSolver {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static readonly string PATH_TO_BIN_PICTURE = Path.Combine("..", "..", "..", "skipper_bin.png");
        private static readonly string SUSPECTED_PASSWORD = "skipper";
        private static readonly string SUSPECTED_PASSWORD_HEX = BitConverter.ToString(Encoding.ASCII.GetBytes(SUSPECTED_PASSWORD)).Replace("-", "");
        private static readonly BlowFish blowFish = new BlowFish(SUSPECTED_PASSWORD);

        private static byte[] ConvertBitToByteArray(BitArray bitArray) {
            if (bitArray.Length % 8 != 0) {
                throw new ArgumentException("Input array doesn't have a length which is a multiple of 8.");
            }
            byte[] byteArray = new byte[bitArray.Length / 8];
            bitArray.CopyTo(byteArray, 0);
            return byteArray;
        }

        static void Main(string[] args) {
            LOGGER.Info("Suspected Password: " + SUSPECTED_PASSWORD);
            LOGGER.Info("As HEX: " + SUSPECTED_PASSWORD_HEX);

            using (Image<Rgb24> binImage = Image.Load<Rgb24>(PATH_TO_BIN_PICTURE)) {
                BitArray cipherBits = new BitArray(binImage.Width * binImage.Height);
                for (int y = 0; y < binImage.Height; y++) {
                    for (int x = 0; x < binImage.Width; x++) {
                        cipherBits[y * 0 + x] = (binImage[x, y].B == 0) ? false : true;
                    }
                }
                LOGGER.Info("Image successfully read.");

                // Bit-to-Byte Test
                byte[] cipherBytes = ConvertBitToByteArray(cipherBits);
                string bits = "";
                for (int i = 0; i < 8; i++) {
                    bits += cipherBits[i] ? "1" : "0";
                }
                LOGGER.Info(bits);
                LOGGER.Info(cipherBytes[0]);

                byte[] plainBytes = blowFish.Decrypt(cipherBytes, System.Security.Cryptography.CipherMode.ECB);
                LOGGER.Warn(Encoding.ASCII.GetString(plainBytes));
            }
        }

    }

}
