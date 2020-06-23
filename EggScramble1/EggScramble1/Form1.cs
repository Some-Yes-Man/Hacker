using System;
using System.Text;
using System.Windows.Forms;

namespace EggScramble1 {
    public partial class Form1 : Form {
        // 140285140285140285998287
        // 140285140285140285998287
        private byte[] key = new byte[4] { 20, 40, 30, 50 };
        private static int SCRAMBLE_WIDTH = 24;
        private static int SCRAMBLE_LOOPS = 3;

        public Form1() {
            InitializeComponent();
        }

        private void btnEncrypt_Click(object sender, EventArgs e) {
            int plainIndex = 0;
            byte[] plainArray = Encoding.ASCII.GetBytes(txtBoxPlain.Text);
            while (plainIndex < (plainArray.Length - 1)) {
                int eggs = plainArray[plainIndex] << 16;
                if ((plainIndex + 1) < (plainArray.Length - 1)) {
                    eggs &= plainArray[plainIndex + 1] << 8;
                }
                if ((plainIndex + 2) < (plainArray.Length - 1)) {
                    eggs &= plainArray[plainIndex + 2];
                }

                int roll = 7;
                for (int i = 0; i < SCRAMBLE_LOOPS; i++) {
                    eggs ^= key[eggs & 0x3] << 8;
                    eggs = (eggs << roll) | (eggs >> (SCRAMBLE_WIDTH - roll));
                    eggs &= ((1 << SCRAMBLE_WIDTH) - 1);
                }

                Console.WriteLine(eggs);
                txtBoxCypher.AppendText(eggs.ToString("X6"));
                plainIndex += 3;
            }
            txtBoxPlain.Clear();
        }

        private void btnDecrypt_Click(object sender, EventArgs e) {
            if (txtBoxCypher.Text.Length % 6 == 1) {
                txtBoxPlain.Text = "Odd cypher text length.";
                return;
            }

            byte[] cypherArray = new byte[txtBoxCypher.Text.Length / 2];
            for (int i = 0; i < txtBoxCypher.Text.Length; i++) {
                cypherArray[i] = Convert.ToByte(txtBoxCypher.Text.Substring(i * 2, 2), 16);
            }
            int cypherIndex = 0;
            while (cypherIndex < (cypherArray.Length - 1)) {
                int eggs = (cypherArray[cypherIndex] << 16) + (cypherArray[cypherIndex] << 8) + cypherArray[cypherIndex];
                Console.WriteLine(eggs);
                cypherIndex += 3;
            }
            //while (plainIndex < (plainArray.Length - 1)) {
            //    int eggs = plainArray[plainIndex] << 16;
            //    if ((plainIndex + 1) < (plainArray.Length - 1)) {
            //        eggs &= plainArray[plainIndex + 1] << 8;
            //    }
            //    if ((plainIndex + 2) < (plainArray.Length - 1)) {
            //        eggs &= plainArray[plainIndex + 2];
            //    }

            //    int roll = 7;
            //    for (int i = 0; i < SCRAMBLE_LOOPS; i++) {
            //        eggs ^= key[eggs & 0x3] << 8;
            //        eggs = (eggs << roll) | (eggs >> (SCRAMBLE_WIDTH - roll));
            //        eggs &= ((1 << SCRAMBLE_WIDTH) - 1);
            //    }

            //    byte[] scrambleEggs = new byte[3];
            //    scrambleEggs[0] = (byte)((eggs >> 16) & 0xff);
            //    scrambleEggs[1] = (byte)((eggs >> 8) & 0xff);
            //    scrambleEggs[2] = (byte)(eggs & 0xff);
            //    txtBoxCypher.AppendText(BitConverter.ToString(scrambleEggs).Replace("-", string.Empty));
            //    plainIndex += 3;
            //}
            //txtBoxPlain.Clear();
        }
    }
}
