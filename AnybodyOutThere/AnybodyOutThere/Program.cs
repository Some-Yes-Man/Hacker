using NLog;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AnybodyOutThere {
    internal class Program {
        private static Logger LOGGER = LogManager.GetCurrentClassLogger();

        const int COUNT_X = 1000;
        const int COUNT_Y = 300;
        const int REQ_X_START = 0;
        const int REQ_Y_START = 0;
        const int REQ_X_COUNT = COUNT_X;
        const int REQ_Y_COUNT = COUNT_Y;
        const int TASK_COUNT = 10;

        private static readonly UriBuilder requestBuilder = new UriBuilder("https://www.hacker.org/challenge/misc/twonumbers.php");
        private static readonly Regex numberParser = new(".*?form>\\s*?(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>.*");
        private static readonly OutThereData[] results = new OutThereData[COUNT_X * COUNT_Y];
        private static readonly Tuple<int, int, Task<HttpResponseMessage>>[] tasks = new Tuple<int, int, Task<HttpResponseMessage>>[TASK_COUNT];

        static void printCsv() {
            for (int x = REQ_X_START; x < REQ_X_START + REQ_X_COUNT; x++) {
                for (int y = REQ_Y_START; y < REQ_Y_START + REQ_Y_COUNT; y++) {
                    string foo = "";
                    for (int r = 0; r < OutThereData.COUNT_RESULT; r++) {
                        foo += results[y * COUNT_X + x].Data[r] + (r < OutThereData.COUNT_RESULT - 1 ? "," : "");
                    }
                    LOGGER.Info(foo);
                }
            }
        }

        static async void getPreviousTaskResult(int index) {
            int modIndex = index % TASK_COUNT;
            if (tasks[modIndex] != null) {
                HttpResponseMessage msg = tasks[modIndex].Item3.Result;

                Match match = numberParser.Match(await msg.Content.ReadAsStringAsync());
                if (match.Success) {
                    for (int i = 0; i < OutThereData.COUNT_RESULT; i++) {
                        results[tasks[modIndex].Item2 * COUNT_X + tasks[modIndex].Item1].Data[i] = int.Parse(match.Groups[i + 1].Value);
                    }
                }
                else {
                    LOGGER.Warn("Found something weird at {x}:{y} ... {weird}", tasks[modIndex].Item1, tasks[modIndex].Item2, match.Groups[0].Value);
                }
            }
        }

        static async Task Main(string[] args) {
            for (int i = 0; i < COUNT_X * COUNT_Y; i++) {
                results[i] = new OutThereData();
            }

            using (HttpClient client = new HttpClient()) {
                int currentIndex = 0;

                for (int x = REQ_X_START; x < REQ_X_START + REQ_X_COUNT; x++) {
                    for (int y = REQ_Y_START; y < REQ_Y_START + REQ_Y_COUNT; y++) {
                        currentIndex++;
                        getPreviousTaskResult(currentIndex);

                        LOGGER.Info("Requesting x:" + x + " y:" + y);
                        requestBuilder.Query = "go=Try&one=" + x + "&two=" + y;
                        tasks[currentIndex % 10] = new Tuple<int, int, Task<HttpResponseMessage>>(x, y, client.GetAsync(requestBuilder.Uri));
                    }
                }

                for (int i = 0; i < TASK_COUNT; i++) {
                    getPreviousTaskResult(i);
                }
                LOGGER.Info("Done.");
            }

            using FileStream fileStream = File.Create("E:\\Yes-Man\\Dropbox\\anybody.json");
            await JsonSerializer.SerializeAsync(fileStream, results, new JsonSerializerOptions { WriteIndented = false });
            await fileStream.DisposeAsync();
        }
    }
}
