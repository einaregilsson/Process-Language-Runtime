using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using PLR.AST.Actions;

namespace PLR {

    public class Scheduler {

        private Scheduler() { }
        private static Scheduler _instance = new Scheduler();
        public static Scheduler Instance {
            get { return _instance; }
        }
        //Active processes and their possible actions
        private Dictionary<ProcessBase, List<IAction>> _activeProcs = new Dictionary<ProcessBase, List<IAction>>();
        private List<Match> _matches = new List<Match>();

        public void SyncActions(ProcessBase proc, List<IAction> possibleActions) {
            lock (this) {
                _activeProcs[proc] = possibleActions;
                Console.WriteLine("Synced actions for " + proc);
            }
        }

        public void AddProcess(ProcessBase p) {
            lock (this) {
                _activeProcs.Add(p, new List<IAction>());
            }
        }

        public void KillProcess(ProcessBase p) {
            lock (this) {
                _activeProcs.Remove(p);
            }
        }

        private class Match {
            public IAction a1, a2;
            public Match(IAction a1, IAction a2) { this.a1 = a1; this.a2 = a2; }
        }

        public void Run() {
            Console.WriteLine("Running Scheduler");
            while (true) {
                Console.WriteLine("Procs: " + _activeProcs.Keys.Count);
                bool allWaiting = true;
                Console.WriteLine(_activeProcs.Keys.Count);
                foreach (ProcessBase p in _activeProcs.Keys) {
                    allWaiting &= p.Waiting;
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
            _matches.Clear();
            for (int i = 0; i < allActions.Count; i++) {
                for (int j = i + 1; j < allActions.Count; j++) {
                    if (allActions[i].CanSyncWith(allActions[j]) 
                        && allActions[i].ProcessID != allActions[j].ProcessID
                        && ProcessIdToSetId(allActions[i].ProcessID) != ProcessIdToSetId(allActions[j].ProcessID)) {

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
            foreach (ProcessBase p in _activeProcs.Keys) {
                if (!wakeUp.Contains(p) && wakeUpGuids.Contains(p.SetID)) {
                    p.ChosenAction = ProcessBase.KILL_YOURSELF;
                    wakeUp.Add(p);
                }
            }

            //Finally, wake everyone up that needs to do something
            foreach (ProcessBase p in wakeUp) {
                Console.WriteLine("Waking up " + p);
                p.Continue();
            }
        }
    }
}
