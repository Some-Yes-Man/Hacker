using Newtonsoft.Json;
using NLog;
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
        const bool REQ_ENABLED = false;

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
                        int currentIndex = tasks[modIndex].Item2 * COUNT_X + tasks[modIndex].Item1;
                        results[currentIndex].X = tasks[modIndex].Item1;
                        results[currentIndex].Y = tasks[modIndex].Item2;
                        results[currentIndex].Data[i] = int.Parse(match.Groups[i + 1].Value);
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

            if (REQ_ENABLED) {
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

                using (StreamWriter streamWriter = new StreamWriter("E:\\Yes-Man\\Dropbox\\anybody.json", new FileStreamOptions { Mode = FileMode.Create })) {
                    using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter)) {
                        JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.None });
                        jsonSerializer.Serialize(jsonWriter, results);
                    }
                }
            }
            else {
                List<OutThereData>? outThereData;
                using (StreamReader reader = new StreamReader("C:\\Users\\robert.krausse\\Dropbox\\anybody.json")) {
                    string jsonString = reader.ReadToEnd();
                    outThereData = JsonConvert.DeserializeObject<List<OutThereData>>(jsonString);
                    if (outThereData != null) {
                        for (int d = 0; d < OutThereData.COUNT_RESULT; d++) {
                            Image<Rgb24> image = new Image<Rgb24>(COUNT_X, COUNT_Y);
                            for (int y = 0; y < COUNT_Y; y++) {
                                image.ProcessPixelRows(source => {
                                    Span<Rgb24> row = source.GetRowSpan(y);
                                    for (int x = 0; x < COUNT_X; x++) {
                                        byte grayValue = (byte)((1000.0 / outThereData[y * COUNT_X + x].Data[d]) * 255);
                                        ref Rgb24 pixel = ref row[x];
                                        pixel.R = grayValue;
                                        pixel.G = grayValue;
                                        pixel.B = grayValue;
                                    }
                                });
                            }
                            LOGGER.Info("Saving data image #" + d + ".");
                            image.SaveAsBmp("data" + d + ".bmp");
                        }
                    }
                }
            }
        }
    }
}
