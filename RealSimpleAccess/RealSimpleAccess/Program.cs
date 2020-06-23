using System;
using System.ComponentModel;
using System.IO;
using System.Numerics;

namespace RealSimpleAccess {
    class Program {

        //const string RESULT_FILENAME = "result.txt";
        const string RESULT_FILENAME = "test.txt";
        const string RSA_MODUL = "18177463113985279014593768153541854004936176165294524057336890660730144311208729";
        //static readonly string RSA_MODUL = (BigInteger.Parse("17180131327") * BigInteger.Parse("1073807359")).ToString();
        //const string RSA_MODUL_ROOT_GUESS = "4263503619558130387420489559062518838706";
        const string RSA_MODUL_ROOT_GUESS = "3";
        const int THREADS = 6;
        const int WORK_LOAD = 100000000;

        private class PrimeFinder {
            BackgroundWorker[] bgWorkers = new BackgroundWorker[ THREADS ];
            BigInteger startingPoint;
            BigInteger rsa_modul = BigInteger.Parse(RSA_MODUL);
            private object counterLock = new object();
            private object fileLock = new object();

            public PrimeFinder(BigInteger start) {
                this.startingPoint = start;
            }

            public void Run() {
                for (int i = 0; i < THREADS; i++) {
                    this.bgWorkers[ i ] = new BackgroundWorker {
                        WorkerReportsProgress = true
                    };
                    this.bgWorkers[ i ].DoWork += PrimeFinder_DoWork;
                    this.bgWorkers[ i ].RunWorkerCompleted += PrimeFinder_RunWorkerCompleted;
                    this.bgWorkers[ i ].ProgressChanged += PrimeFinder_ProgressChanged;
                    lock (this.counterLock) {
                        this.bgWorkers[ i ].RunWorkerAsync(this.startingPoint);
                        //this.startingPoint -= WORK_LOAD;
                        this.startingPoint += WORK_LOAD;
                    }
                }
            }

            private void PrimeFinder_DoWork(object sender, DoWorkEventArgs e) {
                BackgroundWorker worker = sender as BackgroundWorker;
                BigInteger startPosition = (BigInteger) e.Argument;
                BigInteger currentPosition = startPosition;

                //while (currentPosition + WORK_LOAD > startPosition) {
                while (currentPosition < startPosition + WORK_LOAD) {
                    if (BigInteger.Remainder(rsa_modul, currentPosition) == 0) {
                        worker.ReportProgress(100, currentPosition);
                    }
                    if (currentPosition.IsEven) {
                        //currentPosition--;
                        currentPosition++;
                    }
                    else {
                        //currentPosition -= 2;
                        currentPosition += 2;
                    }
                }
                e.Result = startPosition;
            }

            private void PrimeFinder_ProgressChanged(object sender, ProgressChangedEventArgs e) {
                if (e.ProgressPercentage == 100) {
                    BigInteger possibleSolutionA = (BigInteger) e.UserState;
                    BigInteger possibleSolutionB = BigInteger.Parse(RSA_MODUL) / possibleSolutionA;

                    Console.WriteLine();
                    Console.WriteLine("Possible solution:");
                    Console.WriteLine("A: " + possibleSolutionA);
                    Console.WriteLine("B: " + possibleSolutionB);
                    Console.WriteLine("R: " + BigInteger.Remainder(BigInteger.Parse(RSA_MODUL), possibleSolutionA));

                    lock (this.fileLock) {
                        using (StreamWriter writer = File.AppendText(RESULT_FILENAME)) {
                            writer.WriteLine("Found a solution:");
                            writer.WriteLine("A: " + possibleSolutionA);
                            writer.WriteLine("B: " + possibleSolutionB);
                            writer.WriteLine("R: " + BigInteger.Remainder(BigInteger.Parse(RSA_MODUL), possibleSolutionA));
                        }
                    }
                }
                else {
                    Console.Write("#");
                }
            }

            private void PrimeFinder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
                BackgroundWorker worker = sender as BackgroundWorker;
                BigInteger finishedPosition = (BigInteger) e.Result;
                Console.WriteLine("Finished position: " + finishedPosition);
                lock (this.fileLock) {
                    using (StreamWriter writer = File.AppendText(RESULT_FILENAME)) {
                        writer.WriteLine("Finished position: " + finishedPosition);
                    }
                }
                lock (this.counterLock) {
                    worker.RunWorkerAsync(this.startingPoint);
                    this.startingPoint += WORK_LOAD;
                    //this.startingPoint -= WORK_LOAD;
                }
            }
        }
        static void Main(string[] args) {
            //BigInteger rsa_modul = BigInteger.Parse(RSA_MODUL);
            //BigInteger root = BigInteger.Parse("1000");
            //for (int i = 0; i < 50; i++) {
            //    root = ((rsa_modul / root) + root) / 2;
            //    Console.WriteLine("Estimated root of RSA modul: " + root);
            //}
            PrimeFinder finder = new PrimeFinder(BigInteger.Parse(RSA_MODUL_ROOT_GUESS));
            finder.Run();
            Console.ReadKey(true);
        }
    }
}
