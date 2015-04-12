using System;
using System.Media;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WC3GameNamePastR.KeyboardHook;

namespace WC3GameNamePastR
{
    class Program
    {
        private volatile static string _lastGame;

        [STAThread]
        static void Main()
        {
            HotKeyManager.RegisterHotKey(Keys.E, KeyModifiers.Control);
            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

            var webClient = new WebClient { Proxy = null };
            Console.CursorVisible = false;

            do
            {
                var lastGameName = "";
                while (!Console.KeyAvailable)
                {
                    var random = new Random();
                    var url = "https://entgaming.net/forum/games_fast.php?random=" + random.Next();
                    var downloadString = webClient.DownloadString(url);

                    var result = Regex.Match(downloadString,
                        @"(.*?)\|.*?\|.*?\|.*?\|.*?\|(\[ENT\] Legion TD Mega euro.*)");

                    var id = result.Groups[1].Value;
                    var gameName = result.Groups[2].Value;

                    if (String.IsNullOrEmpty(gameName))
                        continue;

                    if (!String.IsNullOrEmpty(lastGameName) && gameName != lastGameName)
                        SystemSounds.Exclamation.Play();

                    _lastGame = lastGameName = gameName;

                    url = "https://entgaming.net/forum/slots_fast.php?id=" + id + "&random=" + random.Next();
                    downloadString = webClient.DownloadString(url);

                    var regexObj = new Regex("<tr>.*?<td.*?>(.*?)</td>", RegexOptions.Singleline);
                    var matchResults = regexObj.Match(downloadString);

                    Console.Clear();
                    Console.WriteLine(gameName);
                    Console.WriteLine();

                    var i = 1;

                    while (matchResults.Success)
                    {
                        var inner = matchResults.Groups[1].Value;

                        var player = Regex.Match(inner, "<a.*?>(.*?)</a>", RegexOptions.Singleline).Groups[1].Value;
                        if (!String.IsNullOrEmpty(player))
                            Console.WriteLine("Slot " + i++ + ": " + player);

                        if (inner == "Empty")
                            Console.WriteLine("Slot " + i++ + ": -");

                        if (i == 5)
                            Console.WriteLine();

                        matchResults = matchResults.NextMatch();
                    }

                    Thread.Sleep(500);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

        }

        private static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            String2VK.SendText(_lastGame);
        }

    }
}
