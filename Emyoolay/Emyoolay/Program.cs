using System;
using System.Text;

namespace Emyoolay {
    class Program {

        //  !"#$%&'()
        // *+,-./0123
        // 456789:;<=
        // >?@ABCDEFG
        // HIJKLMNOPQ
        // RSTUVWXYZ[
        // ]^_`abcdef
        // ghijklmnop
        // qrstuvwxyz
        // {|}~\

        private static readonly string trialInput = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~\\";
        private static readonly string inputAlphabet_ = " !$*+-/0123456789:<>?P^pcdg";
        private static readonly string outputAlphabet = " !$*+-/0123456789:<>?C^cpqt";

        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("No parameter given.");
                return;
            }

            byte[] inputBytes = Encoding.ASCII.GetBytes(args[0]);
            StringBuilder outputString = new StringBuilder();
            for (int inputIndex = 0; inputIndex < inputBytes.Length; inputIndex++) {
                int firstFactor = 9;
                int secondFactor = 0;
                int thirdFactor = 0;
                int addition = 0;

                for (int i = 1; i <= 9; i++) {
                    for (int j = 1; j <= 9; j++) {
                        if ((secondFactor == 0) && (inputBytes[inputIndex] == i * j)) {
                            firstFactor = i;
                            secondFactor = j;
                        }
                    }
                }

                // no perfect match found
                if (secondFactor == 0) {
                    // need more than two factors
                    if (inputBytes[inputIndex] / firstFactor > 9) {
                        for (int i = 1; i <= 9; i++) {
                            for (int j = 1; j <= 9; j++) {
                                if ((inputBytes[inputIndex] % (firstFactor * i * j) <= 9) && (inputBytes[inputIndex] % (firstFactor * i * j) < addition)) {
                                    secondFactor = i;
                                    thirdFactor = j;
                                    addition = inputBytes[inputIndex] % (firstFactor * secondFactor * thirdFactor);
                                }
                            }
                        }
                    }
                    // two factors are enough
                    else {
                        secondFactor = inputBytes[inputIndex] / 9;
                        addition = inputBytes[inputIndex] % secondFactor;
                    }
                }

                outputString.Append(firstFactor).Append(secondFactor).Append('*');
                if (thirdFactor > 0) {
                    outputString.Append(thirdFactor).Append('*');
                }
                if (addition > 0) {
                    outputString.Append(addition).Append('+');
                }
                outputString.Append('P');
            }

            Console.WriteLine(outputString.ToString() + "!");
        }

    }
}
