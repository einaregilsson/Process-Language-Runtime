/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PLR.Runtime {
    static class Logger {

        public static bool SchedulerDebugOn {get; set;}
        public static bool ProcessDebugOn {get; set;}
        public static bool TraceDebugOn {get; set;}
        public static bool TraceDebugIncludeTau { get; set; }
        public static bool TraceDebugIncludeMethodCalls { get; set; }

        static Logger() {
            //Defaults
            SchedulerDebugOn = false;
            ProcessDebugOn = false;
            TraceDebugOn = true;
            TraceDebugIncludeTau = true;
            TraceDebugIncludeMethodCalls = true;
            //Now lets see if they are overwritten
            string[] args = System.Environment.GetCommandLineArgs();
            foreach (string arg in args) {
                string larg = arg.ToLower();
                if (larg.StartsWith("/log:")) {
                    TraceDebugOn = SchedulerDebugOn = ProcessDebugOn = TraceDebugIncludeTau = TraceDebugIncludeMethodCalls = false; //Set all to false and use what's passed in
                    if (larg.Length == 5) {
                        Console.Error.WriteLine("ERROR: Invalid /log: argument");
                        System.Environment.Exit(1);
                    } else {
                        string[] opts = larg.Substring(5).Split(',');
                        foreach (string opt in opts) {
                            if (opt == "trace") {
                                TraceDebugOn = true;
                            } else if (opt == "tau") {
                                TraceDebugIncludeTau = true;
                            } else if (opt == "methods") {
                                TraceDebugIncludeMethodCalls = true;
                            } else if (opt == "scheduler") {
                                SchedulerDebugOn = true;
                            } else if (opt == "process") {
                                ProcessDebugOn = true;
                            } else {
                                Console.Error.WriteLine("ERROR: Invalid /log option: '{0}'", opt);
                                System.Environment.Exit(1);
                            }
                        }

                    }
                }
            }
        }

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
        
        public static void SchedulerDebug(object msg) {
            if (SchedulerDebugOn) {
                Output(msg);
            }
        }

        public static void ProcessDebug(object msg) {
            if (ProcessDebugOn) {
                Output(msg);
            }
        }

        public static void TraceDebug(CandidateAction action) {
            if (action.IsAsync && TraceDebugIncludeMethodCalls) {
                Output("TRACE: " + action);
            } else if (action.IsTau && TraceDebugIncludeTau) {
                Output("TRACE: " + action);
            } else if (action.IsDeadlocked) {
                Output("TRACE: " + action);
            } else if (!action.IsTau && action.IsSync && TraceDebugOn) {
                Output("TRACE: " + action);
            }
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
