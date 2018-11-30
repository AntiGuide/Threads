using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Threads {
    class Program {
        private const string ROOT_URL = "http://www.games-academy.de/";
        private static int ocurrences = 0;
        private static object ocurrencesMutex = new object();
        private static HashSet<string> knownSites = new HashSet<string>();

        static void Main(string[] args) {
            var begin = DateTime.UtcNow;
            knownSites.Add("");
            AnalyzeSite("", 3);
            var end = DateTime.UtcNow;
            Console.WriteLine("Cont of word Game: " + ocurrences);
            Console.WriteLine("The request took " + (end - begin).TotalMilliseconds + " ms.");
            Console.ReadKey();
        }

        private static void AnalyzeSite(string urlPart, byte trys) {
            trys--;
            if (trys < 0) {
                return;
            }

            string content = "";
            using (var wc = new System.Net.WebClient()) {
                try {
                    content = wc.DownloadString(ROOT_URL + urlPart);
                } catch (Exception e) {
                    Console.WriteLine(ROOT_URL + urlPart + ": " + e.Message);
                }
            }

            var tasks = new List<Task>();

            foreach (Match match in Regex.Matches(content, "href=\"/([^\"#\\?:.]*)[\"#\\?]")) {
                lock (knownSites) {
                    if (knownSites.Contains(match.Groups[1].Value) || match.Groups[1].Value.Contains("node")) {
                        continue;
                    } else {
                        knownSites.Add(match.Groups[1].Value);
                    }
                }

                tasks.Add(Task.Run(() => {
                    AnalyzeSite(match.Groups[1].Value, trys);
                }));
            }

            Task.WaitAll(tasks.ToArray());
            var count = Regex.Matches(content, "(?:^|\\W)Game(?:$|\\W)", RegexOptions.IgnoreCase).Count;
            Interlocked.Add(ref ocurrences, count);
            Console.WriteLine("Try Nr. " + (3 - trys) + ": Reading from " + ROOT_URL + urlPart + " Words " + count);

            return;
        }
    }
}
