using NLog;
using System.Text.RegularExpressions;

namespace AnybodyOutThere {
    internal class Program {
        private static Logger LOGGER = LogManager.GetCurrentClassLogger();

        const int COUNT_X = 1000;
        const int COUNT_Y = 300;
        const int COUNT_RESULT = 12;
        const int REQ_X_START = 979;
        const int REQ_Y_START = 279;
        const int REQ_X_COUNT = 20;
        const int REQ_Y_COUNT = 20;
        const int REQ_DELAY = 100;

        static async Task Main(string[] args) {
            UriBuilder requestBuilder = new UriBuilder("https://www.hacker.org/challenge/misc/twonumbers.php");
            Regex numberParser = new Regex(".*?form>\\s*?(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>(\\d+)<br>.*");
            int[,,] results = new int[COUNT_X, COUNT_Y, COUNT_RESULT];

            using (HttpClient client = new HttpClient()) {
                for (int x = REQ_X_START; x <= REQ_X_START + REQ_X_COUNT; x++) {
                    for (int y = REQ_Y_START; y <= REQ_Y_START + REQ_Y_COUNT; y++) {
                        LOGGER.Info("Requesting x:" + x + " y:" + y);
                        requestBuilder.Query = "go=Try&one=" + x + "&two=" + y;

                        HttpResponseMessage foo = await client.GetAsync(requestBuilder.Uri);
                        Match match = numberParser.Match(await foo.Content.ReadAsStringAsync());
                        if (match.Success) {
                            for (int i = 0; i < COUNT_RESULT; i++) {
                                results[x, y, i] = int.Parse(match.Groups[i + 1].Value);
                            }
                        }
                        else {
                            LOGGER.Warn("Found something weird at {x}:{y} ... {weird}", x, y, match.Groups[0].Value);
                        }
                        Thread.Sleep(REQ_DELAY);
                    }
                }
                LOGGER.Info("Done.");
            }
            for (int x = REQ_X_START; x <= REQ_X_START + REQ_X_COUNT; x++) {
                for (int y = REQ_Y_START; y <= REQ_Y_START + REQ_Y_COUNT; y++) {
                    string foo = "";
                    for (int r = 0; r < COUNT_RESULT; r++) {
                        foo += results[x, y, r] + (r < COUNT_RESULT - 1 ? "," : "");
                    }
                    LOGGER.Info(foo);
                }
            }
        }
    }
}
