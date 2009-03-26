using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PLR {
    static class Logger {
        private static List<ConsoleColor> _availableColors = new List<ConsoleColor>(
            new ConsoleColor[] { 
                ConsoleColor.Blue, 
                ConsoleColor.Cyan, 
                ConsoleColor.Green, 
                ConsoleColor.Magenta, 
                ConsoleColor.Red, 
                ConsoleColor.Yellow 
        });
        private static string lockString = "LOCKME";

        //Mapping from threads to colors
        private static Dictionary<int, ConsoleColor> _threadColors = new Dictionary<int, ConsoleColor>();
        private static Dictionary<int, string> _prefixes = new Dictionary<int, string>();

        public static void Register(string prefix) {
            lock (lockString) {
                if (_availableColors.Count > 0)
                {
                    ConsoleColor color = _availableColors[0];
                    _availableColors.RemoveAt(0);
                    _threadColors.Add(Thread.CurrentThread.ManagedThreadId, color);
                    _prefixes.Add(Thread.CurrentThread.ManagedThreadId, prefix);
                }
            }
        }

        public static void Unregister() {
            lock (lockString) {
                if (_threadColors.ContainsKey(Thread.CurrentThread.ManagedThreadId)) {

                    _availableColors.Add(_threadColors[Thread.CurrentThread.ManagedThreadId]);
                    _threadColors.Remove(Thread.CurrentThread.ManagedThreadId);
                    _prefixes.Remove(Thread.CurrentThread.ManagedThreadId);
                }
            }
        }
        
        public static void Debug(object msg) {
            Output(msg);
        }

        public static void context(object msg) {
            Output(msg);
        }

        private static void Output(object msg) {
            string s = msg == null ? "null" : msg.ToString();
            string prefix = "";
            lock (lockString) {
                ConsoleColor original = Console.ForegroundColor;
                if (_threadColors.ContainsKey(Thread.CurrentThread.ManagedThreadId)) {
                    Console.ForegroundColor = _threadColors[Thread.CurrentThread.ManagedThreadId];
                    prefix = _prefixes[Thread.CurrentThread.ManagedThreadId];
                }
                Console.WriteLine(prefix + s);
                Console.ForegroundColor = original;
            }
        }
    }
}
