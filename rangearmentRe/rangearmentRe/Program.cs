using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace rangearmentRe {
    class Program {

        const string myPathString = "C:\\Users\\rkrausse\\Downloads\\manyfiles";

        public static string fakeParseDirectory(string directoryName) {
            string foo = string.Empty;
            // first traverse down as far as possible
            foreach (string subDirectoryName in Directory.GetDirectories(directoryName)) {
                foo += fakeParseDirectory(subDirectoryName);
            }
            foreach (string fileName in Directory.GetFiles(directoryName)) {
                foo += fileName.Substring(fileName.Length - 6, 2);
            }
            return foo;
        }

        public static string realParseDirectory(string directoryName) {
            string foo = string.Empty;
            // first traverse down as far as possible
            foreach (string subDirectoryName in Directory.GetDirectories(directoryName)) {
                foo += realParseDirectory(subDirectoryName);
            }
            foreach (string fileName in Directory.GetFiles(directoryName)) {
                FileInfo fi = new FileInfo(fileName);
                foo += fi.Length.ToString("X2");
            }
            return foo;
        }

        static void Main(string[] args) {
            using (FileStream outputStream = new FileStream("outFake.png", FileMode.Create)) {
                using (BinaryWriter outputWriter = new BinaryWriter(outputStream)) {
                    string bar = fakeParseDirectory(myPathString);
                    for (int i = 0; i < bar.Length; i+=2) {
                        outputWriter.Write(byte.Parse(bar.Substring(i, 2), NumberStyles.HexNumber));
                    }
                }
            }
            Console.WriteLine("Fake done.");
            using (FileStream outputStream = new FileStream("outReal.png", FileMode.Create)) {
                using (BinaryWriter outputWriter = new BinaryWriter(outputStream)) {
                    string bar = realParseDirectory(myPathString);
                    for (int i = 0; i < bar.Length; i += 2) {
                        outputWriter.Write(byte.Parse(bar.Substring(i, 2), NumberStyles.HexNumber));
                    }
                }
            }
            Console.WriteLine("Real done.");
            Console.ReadKey(true);
        }
    }
}
