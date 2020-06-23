using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading;

namespace PrimalPi {
    class Program {

        const string PI_2048 = "14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327886593615338182796823030195203530185296899577362259941389124972177528347913151557485724245415069595082953311686172785588907509838175463746493931925506040092770167113900984882401285836160356370766010471018194295559619894676783744944825537977472684710404753464620804668425906949129331367702898915210475216205696602405803815019351125338243003558764024749647326391419927260426992279678235478163600934172164121992458631503028618297455570674983850549458858692699569092721079750930295532116534498720275596023648066549911988183479775356636980742654252786255181841757467289097777279380008164706001614524919217321721477235014144197356854816136115735255213347574184946843852332390739414333454776241686251898356948556209921922218427255025425688767179049460165346680498862723279178608578438382796797668145410095388378636095068006422512520511739298489608412848862694560424196528502221066118630674427862203919494504712371378696095636437191728746776465757396241389086583264599581339047802759009946576407895126946839835259570982582262052248940";
        const int THREADS = 6;

        // Random generator (thread safe)
        private static ThreadLocal<Random> s_Gen = new ThreadLocal<Random>(
          () => {
              return new Random();
          }
        );

        // Random generator (thread safe)
        private static Random Gen {
            get {
                return s_Gen.Value;
            }
        }

        private static bool IsProbablyPrime(BigInteger value, int witnesses = 10) {
            if (value <= 1)
                return false;

            if (witnesses <= 0)
                witnesses = 10;

            BigInteger d = value - 1;
            int s = 0;

            while (d % 2 == 0) {
                d /= 2;
                s += 1;
            }

            byte[] bytes = new byte[ value.ToByteArray().LongLength ];
            BigInteger a;

            for (int i = 0; i < witnesses; i++) {
                do {
                    Gen.NextBytes(bytes);

                    a = new BigInteger(bytes);
                }
                while (a < 2 || a >= value - 2);

                BigInteger x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1)
                    continue;

                for (int r = 1; r < s; r++) {
                    x = BigInteger.ModPow(x, 2, value);

                    if (x == 1)
                        return false;
                    if (x == value - 1)
                        break;
                }

                if (x != value - 1)
                    return false;
            }

            return true;
        }

        private class PiTester {
            BackgroundWorker[] bgWorkers = new BackgroundWorker[ THREADS ];
            Primality primalityTester = new Primality();
            long possibilityCount = 0;
            long currentLength = 2048; // solution length was 1977

            public void Run() {
                for (int i = 0; i < THREADS; i++) {
                    this.bgWorkers[ i ] = new BackgroundWorker();
                    this.bgWorkers[ i ].DoWork += PiTester_DoWork;
                    this.bgWorkers[ i ].RunWorkerCompleted += PiTester_RunWorkerCompleted;
                    this.bgWorkers[ i ].RunWorkerAsync(Interlocked.Decrement(ref this.currentLength));
                }


                Console.WriteLine("Possibilites: " + possibilityCount);
            }

            class PiWorkResult {
                public long Length { get; private set; }
                public List<string> Results { get; private set; }
                public PiWorkResult(long length) {
                    this.Length = length;
                    this.Results = new List<string>();
                }
            }

            private void PiTester_DoWork(object sender, DoWorkEventArgs e) {
                long length = (long) e.Argument;
                PiWorkResult result = new PiWorkResult(length);

                Console.WriteLine("Trying length " + length + "...");
                for (long j = 0; j <= 2048 - length; j++) {
                    string currentSubString = PI_2048.Substring((int)j, (int)length);
                    if (currentSubString.EndsWith("2") || currentSubString.EndsWith("4") || currentSubString.EndsWith("6") || currentSubString.EndsWith("8") || currentSubString.EndsWith("0") || currentSubString.EndsWith("5")) {
                        continue;
                    }
                    BigInteger currentNumber = BigInteger.Parse(currentSubString);
                    if ((BigInteger.Remainder(BigInteger.Subtract(currentNumber, 1), 4) != 0) && (BigInteger.Remainder(BigInteger.Subtract(currentNumber, 3), 4) != 0)) {
                        continue;
                    }
                    if ((BigInteger.Remainder(BigInteger.Subtract(currentNumber, 1), 6) != 0) && (BigInteger.Remainder(BigInteger.Add(currentNumber, 1), 6) != 0)) {
                        continue;
                    }
                    if (primalityTester.IsPrimeMillerRabin(currentNumber)) {
                        result.Results.Add(currentNumber.ToString());
                    }
                }

                e.Result = result;
            }

            private void PiTester_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
                BackgroundWorker worker = (BackgroundWorker) sender;
                PiWorkResult result = (PiWorkResult) e.Result;
                Console.WriteLine("Completed length " + result.Length + " finished.");
                foreach (string possiblePrimeString in result.Results) {
                    Console.WriteLine(possiblePrimeString);
                }
                if (Interlocked.Read(ref this.currentLength) > 600) {
                    worker.RunWorkerAsync(Interlocked.Decrement(ref this.currentLength));
                }
            }
        }

        static void Main(string[] args) {
            PiTester piTester = new PiTester();
            piTester.Run();
            Console.ReadKey(true);
        }
    }
}
