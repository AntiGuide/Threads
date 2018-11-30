using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Threads {
    class Program {
        private const string ROOT_URL = "http://www.games-academy.de/";
        private static int ocurrences = 0;
        private static object ocurrencesMutex = new object();
        private static HashSet<string> knownSites = new HashSet<string>();
        //private static object knownSitesMutex = new object();
        //public static volatile int i = 0;
        //private static object mutex = new object();

        static void Main(string[] args) {
            var begin = DateTime.UtcNow;
            knownSites.Add("");
            AnalyzeSite("", 3);
            var end = DateTime.UtcNow;
            Console.WriteLine("Cont of word Game: " + ocurrences);
            Console.WriteLine("The request took " + (end - begin).TotalMilliseconds + " ms.");
            Console.ReadKey();
        }

        /// <returns>Marks wether or not to continue</returns>
        private static void AnalyzeSite(string urlPart, byte trys) {
            trys--;
            if (trys < 0) {
                return;
            }

            string content = "";
            using (var wc = new System.Net.WebClient()) {
                try {
                    Console.WriteLine("Try Nr. " + (3 - trys) + ": Reading from " + ROOT_URL + urlPart);
                    content = wc.DownloadString(ROOT_URL + urlPart);
                } catch (Exception e) {
                    Console.WriteLine(ROOT_URL + urlPart + ": " + e.Message);
                }
            }

            var threads = new List<System.Threading.Thread>();

            foreach (Match match in Regex.Matches(content, "href=\"/([^\"#\\?:.]*)[\"#\\?]")) {
                //var link = ROOT_URL + match.Groups[1].Value;
                lock (knownSites) {
                    if (knownSites.Contains(match.Groups[1].Value) || match.Groups[1].Value.Contains("node")) {
                        continue;
                    } else {
                        knownSites.Add(match.Groups[1].Value);
                    }
                }

                threads.Add(new System.Threading.Thread(() => {
                    AnalyzeSite(match.Groups[1].Value, trys);
                }));
            }

            foreach (var t in threads) {
                t.Start();
            }

            foreach (var t in threads) {
                t.Join();
            }

            lock (ocurrencesMutex) {
                ocurrences += Regex.Matches(content, "(?:^|\\W)Game(?:$|\\W)", RegexOptions.IgnoreCase).Count;
            }

            return;
        }
    }
}
