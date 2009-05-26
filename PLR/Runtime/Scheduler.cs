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

using PLR.AST.Actions;

namespace PLR.Runtime {

    public class Scheduler {

        private Scheduler() {
            foreach (string arg in Environment.GetCommandLineArgs()) {
                if (arg.ToLower() == "/i" || arg.ToLower() == "/interactive") {
                    this.Interactive = true;
                }
            }
        }
        private List<IAction> _trace = new List<IAction>();
        private static Scheduler _instance = new Scheduler();

        public static Scheduler Instance {
            get { return _instance; }
        }
        //Active processes and their possible actions
        private List<ProcessBase> _activeProcs = new List<ProcessBase>();
        public bool Interactive { get; set; }

        public void AddProcess(ProcessBase p) {
            Debug("Added " + p);
            lock (_activeProcs) {
                _activeProcs.Add(p);
            }
        }

        public void KillProcess(ProcessBase p) {
            lock (_activeProcs) {
                _activeProcs.Remove(p);
                Debug("Killed " + p);
            }
        }

        private class Match {
            public IAction a1, a2;
            public Match(IAction a1, IAction a2) { this.a1 = a1; this.a2 = a2; }
        }

        public void Run() {
            Debug("Running Scheduler");
            lock (_activeProcs) {
                foreach (ProcessBase p in _activeProcs) {
                    p.Run();
                }
            }
            while (true) {
                Debug("Procs: " + _activeProcs.Count);
                bool allWaiting = true;
                lock (_activeProcs) {
                    foreach (ProcessBase p in _activeProcs) {
                        allWaiting &= (p.State == ThreadState.Suspended || p.State == ThreadState.WaitSleepJoin);
                        Debug(p + " is in state: " + p.State);
                    }
                }
                if (allWaiting) {
                    FindMatches();
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        private Guid ProcessIdToSetId(int processID) {
            foreach (ProcessBase p in _activeProcs) {
                if (p.ID == processID) {
                    return p.SetID;
                }
            }
            return Guid.Empty;
        }

        private void Debug(object msg) {
            Logger.SchedulerDebug("SCHED: " + msg);
        }

        private List<Match> FindMatches(List<IAction> actions) {
            List<Match> candidates = new List<Match>();
            for (int i = 0; i < actions.Count; i++) {
                if (actions[i].IsAsynchronous) {
                    candidates.Add(new Match(actions[i], null));
                } else {
                    for (int j = i + 1; j < actions.Count; j++) {
                        int p1id = actions[i].ProcessID, p2id = actions[j].ProcessID;
                        Guid p1setID = ProcessIdToSetId(p1id), p2setID = ProcessIdToSetId(p2id);
                        if (actions[i].CanSyncWith(actions[j])
                            && (p1setID != p2setID || p1setID == Guid.Empty)) {
                            candidates.Add(new Match(actions[i], actions[j]));
                        }
                    }
                }
            }
            return candidates;
        }

        public void FindMatches() {
            List<ProcessBase> finished = new List<ProcessBase>();
            List<Match> matches = new List<Match>();


            foreach (ProcessBase proc in _activeProcs) {
                ProcessBase parent = proc;
                while (parent != null) {
                    if (!finished.Contains(parent)) {
                        Debug(parent + " contains " + parent.LocalActions.Count + " candidate actions: " + Util.Join(", ", parent.LocalActions));
                        matches.AddRange(FindMatches(parent.LocalActions));
                        finished.Add(parent);
                    }
                    parent = parent.Parent;

                }
            }

            Debug("Global scope contains " + GlobalScope.Actions.Count + " candidate actions: " + Util.Join(", ", GlobalScope.Actions));
            matches.AddRange(FindMatches(GlobalScope.Actions));

            if (matches.Count == 0) {
                //Give blocked processes a chance 
                List<ProcessBase> blockedProcs = new List<ProcessBase>();
                foreach (ProcessBase p in _activeProcs) {
                    if (p.State == ThreadState.WaitSleepJoin) {
                        blockedProcs.Add(p);
                    }
                }
                if (blockedProcs.Count > 0) {
                    Thread.Sleep(1000); //give them a second to see if latest developments allowed them to unblock...
                    foreach (ProcessBase p in blockedProcs) {
                        if (p.State != ThreadState.WaitSleepJoin) {
                            Debug("A process came out of blocked state, all hope is not lost!");
                            return;
                        }
                    }
                }

                Debug("System is deadlocked");
                Logger.TraceDebug("<DEADLOCKED>");
                Console.ReadKey();
                Environment.Exit(1);
            }

            Match m;
            if (Interactive) {
                m = ChooseMatchInterActive(matches);
            } else {
                m = matches[new Random().Next(matches.Count)];
            }
            Debug("Chose match");
            _trace.Add(m.a1);

            //Now let them sync with each other
            if (m.a2 != null && !m.a1.IsAsynchronous && !m.a2.IsAsynchronous) {
                m.a1.Sync(m.a2);
                m.a2.Sync(m.a1);
            }

            List<ProcessBase> wakeUp = new List<ProcessBase>();
            List<Guid> wakeUpGuids = new List<Guid>();
            foreach (ProcessBase p in _activeProcs) {
                if (m.a1.ProcessID == p.ID) {
                    p.ChosenAction = m.a1;
                    wakeUp.Add(p);
                    wakeUpGuids.Add(p.SetID);
                } else if (m.a2 != null && m.a2.ProcessID == p.ID) {
                    p.ChosenAction = m.a2;
                    wakeUp.Add(p);
                    wakeUpGuids.Add(p.SetID);
                }
            }

            if (wakeUp.Count == 1) {
                Debug("Chose async action " + m.a1.ToString() + ", proc " + wakeUp[0] + ".");
            } else {
                Debug("Chose match " + m.a1.ToString().Replace("_", "") + ", procs " + wakeUp[0] + " and " + wakeUp[1]);
            }

            string[] tmp = new String[_trace.Count];
            for (int i = 0; i < _trace.Count; i++) {
                tmp[i] = _trace[i].ToString().Replace("_", "");
            }
            Debug("TRACE: " + "\n     " + String.Join("\n     ", tmp));
            Logger.TraceDebug(tmp[tmp.Length - 1]);
            foreach (ProcessBase p in _activeProcs) {
                if (!wakeUp.Contains(p) && wakeUpGuids.Contains(p.SetID)) {
                    p.ChosenAction = null;
                    wakeUp.Add(p);
                }
            }

            //Finally, wake everyone up that needs to do something
            foreach (ProcessBase p in wakeUp) {
                Debug("Waking up " + p);
                p.Continue();
            }
        }

        private Match ChooseMatchInterActive(List<Match> matches) {

            int choice = -1;
            Console.WriteLine("\nInteractive mode: ");
            for (int i = 0; i < matches.Count; i++) {
                Match printm = matches[i];
                Console.WriteLine((i + 1) + ". " + printm.a1.ToString().Replace("_", ""));
            }
            Console.WriteLine();
            while (choice < 1 || choice > matches.Count) {
                Console.Write("Type the number of the next action to perform: ");
                string input = Console.ReadLine();
                if (!int.TryParse(input, out choice)) {
                    choice = -1;
                }
            }
            return matches[choice - 1];
        }
    }
}
