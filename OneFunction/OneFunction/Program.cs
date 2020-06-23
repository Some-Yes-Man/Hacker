using System;
using System.Text;

namespace OneFunction {
    class Program {

        // 55 48 89 E5 C7 45 F8 77 47 DF 77 C7 45 FC 61 BB 5F 22 48 C7 45 F0 93 19 00 00 48 8B 45 F0 48 01 45 F8 EB 1B 48 81 6D F8 A8 96 28 6B 48 B8 36 AB 4D B0 68 C9 32 22 48 31 45 F8 48 83 6D F0 01 48 83 7D F0 00 75 DE 48 8B 45 F8 C9 C3

        // push rbp
        // mov rbp, rsp
        // mov dword ptr [rbp - 8], 0x77df4777
        // mov dword ptr [rbp - 4], 0x225fbb61
        // mov qword ptr [rbp - 0x10], 0x1993
        // mov rax, qword ptr [rbp - 0x10]
        // add qword ptr [rbp - 8], rax
        // jmp 0x3f
        // sub qword ptr [rbp - 8], 0x6b2896a8
        // movabs rax, 0x2232c968b04dab36
        // xor qword ptr [rbp - 8], rax
        // sub qword ptr [rbp - 0x10], 1
        // cmp qword ptr [rbp - 0x10], 0
        // jne 0x24
        // mov rax, qword ptr [rbp - 8]
        // leave
        // ret

        static void Main(string[] args) {
            ulong initial = 0x225fbb6177df4777;
            ulong foo = 0x1993;
            ulong value = initial + foo;
            for (ulong i = 0; i < foo; i++) {
                value -= 0x6b2896a8;
                value ^= 0x2232c968b04dab36;
            }
            string hexString = value.ToString("X2");
            byte[] solutionBytes = new byte[hexString.Length / 2];
            for (int i = hexString.Length - 2; i > -1; i -= 2) {
                solutionBytes[6 - (i / 2)] = byte.Parse(hexString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            Console.WriteLine(Encoding.ASCII.GetString(solutionBytes));
        }

    }
}
