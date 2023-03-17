using System.Collections;

namespace BmpBppConvert {
    internal class Program {
        const int SIZE_GENERIC_HEADER = 18;

        static int AddUpBytes(byte[] bytes, int offset, int length) {
            byte[] partial = new byte[length];
            Array.Copy(bytes, offset, partial, 0, length);
            return AddUpBytes(partial);
        }

        static int AddUpBytes(params byte[] bytes) {
            int sum = 0;
            for (int i = 0; i < bytes.Length; i++) {
                sum += bytes[i] * (int)Math.Pow(256, i);
            }
            return sum;
        }

        static byte AddUpBits(BitArray bits, int offset, int length) {
            byte sum = 0;
            for (int i = 0; i < length; i++) {
                sum += bits[offset + i] ? (byte)Math.Pow(2, length - i - 1) : (byte)0;
            }
            return sum;
        }

        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Syntax: BmpBppConvert [InputFile] [OutputFile]");
                return;
            }

            if (!File.Exists(args[0])) {
                Console.WriteLine("Input file not found.");
                return;
            }

            using FileStream inputStream = new(args[0], FileMode.Open, FileAccess.Read);
            int bytesRead = 0;

            // read generic header
            byte[] bufferGenericHeader = new byte[SIZE_GENERIC_HEADER];
            bytesRead += inputStream.Read(bufferGenericHeader, 0, SIZE_GENERIC_HEADER);
            int fileSize = AddUpBytes(bufferGenericHeader, 2, 4);
            int dataOffset = AddUpBytes(bufferGenericHeader, 10, 4);
            int headerSize = AddUpBytes(bufferGenericHeader, 14, 4);

            // read specific header (minus the size, which is already in the generic header)
            byte[] bufferSpecificHeader = new byte[headerSize - 4];
            bytesRead += inputStream.Read(bufferSpecificHeader, 0, headerSize - 4);

            // get BPP, depending on header
            int bitPerPixel;
            int imageWidth;
            int imageHeight;
            int pixelCount;
            switch (headerSize) {
                case 40:
                    bitPerPixel = AddUpBytes(bufferSpecificHeader, 10, 2);
                    imageWidth = AddUpBytes(bufferSpecificHeader, 0, 4);
                    imageHeight = AddUpBytes(bufferSpecificHeader, 4, 4);
                    pixelCount = imageWidth * imageHeight;
                    break;
                default:
                    Console.WriteLine("Unknown header type/size (" + headerSize + ").");
                    return;
            }

            // read optional headers + palettes etc
            byte[] bufferOptionalHeaders = new byte[dataOffset - bytesRead];
            bytesRead += inputStream.Read(bufferOptionalHeaders, 0, dataOffset - bytesRead);

            // read data
            int dataCount = fileSize - dataOffset;
            byte[] data = new byte[dataCount];
            bytesRead += inputStream.Read(data, 0, dataCount);

            // convert to basis 8
            BitArray bits = new(data.Length * 8);
            // fill bit array manually, to establish proper order
            for (int i = 0; i < data.Length; i++) {
                byte tmp = data[i];
                for (int j = 0; j < 8; j++) {
                    if (tmp >= Math.Pow(2, 7 - j)) {
                        bits[i * 8 + j] = true;
                        tmp -= (byte)Math.Pow(2, 7 - j);
                    }
                }
            }

            byte[] properData = new byte[pixelCount];
            for (int i = 0; i < pixelCount; i++) {
                byte pixelValue = AddUpBits(bits, bitPerPixel * i, bitPerPixel);
                properData[i] = pixelValue;
            }

            // update BPP to 8
            bufferSpecificHeader[10] = 8;
            bufferSpecificHeader[11] = 0;
            // update data size
            bufferSpecificHeader[16] = (byte)(properData.Length / 256);
            bufferSpecificHeader[17] = (byte)(properData.Length - (bufferSpecificHeader[16] * 256));
            // update file size
            bufferGenericHeader[2] = (byte)((properData.Length + dataOffset) / 256);
            bufferGenericHeader[3] = (byte)((properData.Length + dataOffset) - (bufferSpecificHeader[2] * 256));

            // write new file
            using FileStream outputStream = new(args[1], FileMode.Create, FileAccess.Write);
            outputStream.Write(bufferGenericHeader, 0, bufferGenericHeader.Length);
            outputStream.Write(bufferSpecificHeader, 0, bufferSpecificHeader.Length);
            outputStream.Write(bufferOptionalHeaders, 0, bufferOptionalHeaders.Length);
            outputStream.Write(properData, 0, properData.Length);
        }
    }
}