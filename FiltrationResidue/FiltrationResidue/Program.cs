using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace FiltrationResidue {

    /**
     * The original image contains a data stream in the length of the chunks.
     * It translates to the following text:
     * 
   6. Filter Algorithms 
 
   This chapter describes the filter algorithms that can be applied 
   before compression.  The purpose of these filters is to prepare the 
   image data for optimum compression. 
 
   6.1. Filter types 
 
      PNG filter method 0 defines five basic filter types: 
 
         Type    Name    Meaning 
 
         0       None    11 
         1       Sub     010 
         2       Up      011 
         3       Average 10 
         4       Paeth   00 
 
      (Note that filter method 0 in IHDR specifies exactly this set of 
      five filter types.  If the set of filter types is ever extended, a 
      different filter method number will be assigned to the extended 
      set, so that decoders need not decompress the data to discover 
      that it contains unsupported filter types.) 
 
      The encoder can choose which of these filter algorithms to apply 
      on a scanline-by-scanline basis.  In the image data sent to the 
      compression step, each scanline is preceded by a filter-type byte 
      that specifies the filter algorithm used for that scanli
     *
     */

    class Program {
        public static void Decompress(FileInfo fileToDecompress) {
            using (FileStream originalFileStream = fileToDecompress.OpenRead()) {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName)) {
                    using (DeflateStream decompressionStream = new DeflateStream(originalFileStream, CompressionMode.Decompress)) {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
            }
        }

        static void Main(string[] args) {
            Decompress(new FileInfo("C:\\Users\\rkrausse\\Dropbox\\Work\\residueDataStreamOnly.bin"));
        }
    }
}
