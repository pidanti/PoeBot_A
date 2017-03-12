using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace PoeBot_A {
    internal class WhisperHandler {
        public delegate void Handler(DateTime date, string name, string message);

        public static event Handler NewMessage;

        private const int Delay = 200;

        private static bool _shouldStop;

        private static int _threadCount;
        private const int MaxThreadCount = 10;

        static WhisperHandler() {
            Restart();
        }

        public static void Stop() {
            _shouldStop = true;
        }

        public static void Restart() {

            if (_threadCount > MaxThreadCount) {
                throw new Exception("Too many WhisperHandler threads");
            }

            _threadCount++;
            new Thread(HandleLog).Start();
        }

        private static void HandleLog() {
            var whisperRegex = new Regex(@"(\d{4}\/\d{2}\/\d{2} \d{2}:\d{2}:\d{2}) .*@From (?:<.+> )?([a-zA-Zа-яА-ЯёЁ]{3,32}): (.+)\r\n");

            var stream = File.Open(@"C:\Program Files (x86)\Grinding Gear Games\Path of Exile\logs\Client.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            var reader = new StreamReader(stream); 

            //pass to end
            reader.ReadToEnd();
            
            while (!_shouldStop) {

                Thread.Sleep(Delay);

                //can be broken while poe is logging 
                var text = reader.ReadToEnd();
                if (text.Length == 0) {
                    continue;
                }

                var matches = whisperRegex.Matches(text);

                if (matches.Count == 0) {

                    if (text[3] != '7') {
                        throw new Exception("sliced by file system message in WhisperHandler");
                    }
                    continue;
                }

                foreach (Match match in matches) {

                    var groups = match.Groups;

                    NewMessage?.Invoke(DateTime.Parse(groups[1].Value), groups[2].Value, groups[3].Value);
                }
            }

            reader.Close();
            stream.Close();
        }
    }
}
