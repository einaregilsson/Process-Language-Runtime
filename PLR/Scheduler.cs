using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using PLR.AST.Actions;

namespace PLR {

    public class Scheduler {

        private Scheduler() { }
        private List<IAction> _trace = new List<IAction>();
        private static Scheduler _instance = new Scheduler();
        public static Scheduler Instance {
            get { return _instance; }
        }
        //Active processes and their possible actions
        private Dictionary<ProcessBase, List<IAction>> _activeProcs = new Dictionary<ProcessBase, List<IAction>>();
        private List<Match> _matches = new List<Match>();

        public void SyncActions(ProcessBase proc, List<IAction> possibleActions) {
            lock (_activeProcs) {
                _activeProcs[proc] = possibleActions;
                Console.WriteLine("Synced actions for " + proc);
            }
        }

        public void AddProcess(ProcessBase p) {
            Console.WriteLine("Added " + p);
            lock (_activeProcs) {
                _activeProcs.Add(p, new List<IAction>());
            }
        }

        public void KillProcess(ProcessBase p) {
            lock (_activeProcs) {
                _activeProcs.Remove(p);
                Console.WriteLine("Killed " + p);
            }
        }

        private class Match {
            public IAction a1, a2;
            public Match(IAction a1, IAction a2) { this.a1 = a1; this.a2 = a2; }
        }

        public void Run() {
            Console.WriteLine("Running Scheduler");
            lock (_activeProcs) {
                foreach (ProcessBase p in _activeProcs.Keys) {
                    p.Run();
                }
            }
            while (true) {
                Console.WriteLine("Procs: " + _activeProcs.Keys.Count);
                bool allWaiting = true;
                lock (_activeProcs) {
                    foreach (ProcessBase p in _activeProcs.Keys) {
                        allWaiting &= p.State == ThreadState.Suspended;
                        Console.WriteLine(p + " is in state: " + p.State);
                    }
                }
                
                if (allWaiting) {
                    FindMatches();
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        private Guid ProcessIdToSetId(int processID) {
            foreach (ProcessBase p in _activeProcs.Keys) {
                if (p.ID == processID) {
                    return p.SetID;
                }
            }
            return Guid.Empty;
        }
        public void FindMatches() {
            List<IAction> allActions = new List<IAction>();
            foreach (List<IAction> actions in _activeProcs.Values) {
                allActions.AddRange(actions);
            }
            Console.Write("ALL ACTIONS: ");
            foreach (IAction a in allActions) Console.Write(a + ", ");
            Console.WriteLine();
            _matches.Clear();
            Console.WriteLine("Finding matches");
            for (int i = 0; i < allActions.Count; i++) {
                for (int j = i + 1; j < allActions.Count; j++) {
                    int p1id = allActions[i].ProcessID, p2id = allActions[j].ProcessID;
                    Guid p1setID = ProcessIdToSetId(p1id), p2setID = ProcessIdToSetId(p2id);
                    if (allActions[i].CanSyncWith(allActions[j]) 
                        && (p1setID != p2setID || p1setID == Guid.Empty)) {
                        _matches.Add(new Match(allActions[i], allActions[j]));
                    }
                }
            }
            if (_matches.Count == 0) {
                Console.WriteLine("System is deadlocked");
                Console.ReadKey();
                Environment.Exit(1);
            }

            Match m = _matches[new Random().Next(_matches.Count)];
            _trace.Add(m.a1);
            List<ProcessBase> wakeUp = new List<ProcessBase>();
            List<Guid> wakeUpGuids = new List<Guid>();
            foreach (ProcessBase p in _activeProcs.Keys) {
                if (p.ID == m.a1.ProcessID || p.ID == m.a2.ProcessID) {
                    List<IAction> procActions = _activeProcs[p];
                    for (int i = 0; i < procActions.Count; i++) {
                        if (procActions[i] == m.a1 || procActions[i] == m.a2) {
                            p.ChosenAction = i;
                            wakeUp.Add(p);
                            wakeUpGuids.Add(p.SetID);
                        }
                    }
                }
            }


            Console.WriteLine("Chose match " + m.a1.ToString() + ", procs " + m.a1.ProcessID + " and " + m.a2.ProcessID);
            string[] tmp = new String[_trace.Count];
            for (int i = 0; i < _trace.Count ; i++) {
                tmp[i] = _trace[i].ToString();
            }
            Console.WriteLine("TRACE: " + String.Join(", ", tmp));
            foreach (ProcessBase p in _activeProcs.Keys) {
                if (!wakeUp.Contains(p) && wakeUpGuids.Contains(p.SetID)) {
                    p.ChosenAction = ProcessBase.KILL_YOURSELF;
                    wakeUp.Add(p);
                }
            }

            //Finally, wake everyone up that needs to do something
            foreach (ProcessBase p in wakeUp) {
                Console.WriteLine("Waking up " + p);
                Console.WriteLine(p.SetID);
                p.Continue();
            }
        }
    }
}
