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
        private static List<string> knownSites = new List<string>();
        static void Main(string[] args) {
            var begin = DateTime.UtcNow;
            AnalyzeSite("", 3);
            var end = DateTime.UtcNow;
            Console.WriteLine("Cont of word Game: " + ocurrences);
            Console.WriteLine("The request took " + (end - begin).TotalMilliseconds + " ms.");
            Console.ReadKey();
        }

        /// <returns>Marks wether or not to continue</returns>
        private static bool AnalyzeSite(string urlPart, byte trys) {
            trys--;
            if (trys < 0) {
                return false;
            }

            knownSites.Add(urlPart);

            string content = "";
            using (var wc = new System.Net.WebClient()) {
                try {
                    Console.WriteLine("Reading from " + ROOT_URL + urlPart);
                    content = wc.DownloadString(ROOT_URL + urlPart);
                } catch (Exception e) {
                    Console.WriteLine(ROOT_URL + urlPart + ": " + e.Message);
                }
            }

            foreach (Match match in Regex.Matches(content, "href=\"/([^\"#\\?:.]*)[\"#\\?]")) {
                //var link = ROOT_URL + match.Groups[1].Value;
                if (knownSites.Contains(match.Groups[1].Value) || match.Groups[1].Value.Contains("node")) {
                    continue;
                }

                AnalyzeSite(match.Groups[1].Value, trys);
            }

            ocurrences += Regex.Matches(content, "(?:^|\\W)Game(?:$|\\W)", RegexOptions.IgnoreCase).Count;

            return true;
        }
    }
}
