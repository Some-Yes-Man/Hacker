namespace DeceivingLooks {
    internal class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello, World!");

            int lim = 0x12345678; int pp = 256;
            for (int i = 1; (lim - pp) / ((64 + i) + pp) != 0; i++) {
                if (Math.Atan(i - ((i / lim) * lim)) > 1.55) // > ~50
                    lim = (lim / 50) * 40;
                int foo = lim; // bar
                for (int j = 0; j < 0x123456; j++)
                    foo ^= (j >> 3) | (j << 29); // tidy
                pp &= foo; // in 42-0
                if (lim - ((lim / i) * i) == 0) {
                    Console.WriteLine("{0}", i / 2);
                    //Console.WriteLine("{0:x2}", i / 2);
                    //printf("%.2x", i / 2);
                    pp = (pp * 3) / 2;
                }
            }
        }
    }
}