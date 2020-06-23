using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doll2 {
    class Program {
        static void Main(string[] args) {
            string doll = "3:5)0(1);5)1)2(3]7(3]0.;11));6)0)2(66)7)3]3]2(0]6.S32]9;)d";
            byte key = 3;
            byte[] dollBytes = Encoding.ASCII.GetBytes(doll);
            for (int i = 0; i < dollBytes.Length; i++) {
                dollBytes[i] = (byte)(dollBytes[i] ^ key);
            }
            Console.WriteLine(Encoding.ASCII.GetString(dollBytes));
            Console.ReadKey(true);
        }
    }
}
