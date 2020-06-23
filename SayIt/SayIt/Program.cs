using NAudio.Wave;
using NLog;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace SayIt {
    public class Program {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static readonly string PATH_TO_MP3 = Path.Combine("..", "..", "..", "sayIt.mp3");
        private const int BUFFER_SIZE = 1024;

        public class SayItParser {

            public void Run() {
                LOGGER.Info("Running...");

                using (AudioFileReader audioFile = new AudioFileReader(PATH_TO_MP3))
                using (WaveOutEvent outputDevive = new WaveOutEvent()) {
                    IWaveProvider waveHead16bit = audioFile.Take(TimeSpan.FromMilliseconds(1000)).ToWaveProvider16();
                    byte[] buffer = new byte[BUFFER_SIZE];
                    int currentPosition = 0;
                    int bytesRead = -1;
                    while (bytesRead != 0) {
                        bytesRead = waveHead16bit.Read(buffer, 0, BUFFER_SIZE);
                        StringBuilder stringBuilder = new StringBuilder();
                        for (int i = 0; i < bytesRead; i++) {
                            stringBuilder.Append(buffer[i].ToString("X2"));
                        }
                        LOGGER.Debug(stringBuilder.ToString());
                        currentPosition += bytesRead;
                    }

                    //outputDevive.Init(audioFile.ToMono(1.0f, 0.0f).ToWaveProvider16());
                    //outputDevive.Play();
                    //Thread.Sleep(5000);
                    //outputDevive.Stop();
                }
            }
        }

        static void Main(string[] args) {
            LOGGER.Info("Starting...");

            SayItParser sayItParser = new SayItParser();
            sayItParser.Run();
        }

    }
}
