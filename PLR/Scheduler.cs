using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using PLR.AST.Actions;

namespace PLR {
    
    public class Scheduler {

        private List<ProcessBase> _activeProcs = new List<ProcessBase>();
        private List<IAction> _actions = new List<IAction>();
        private List<Match> _matches = new List<Match>();

        public void SyncActions(params IAction[] possibleActions) {
            lock (this) {
                _actions.AddRange(possibleActions);
            }
        }

        public void AddProcess(ProcessBase p) {
            lock (this) {
                _activeProcs.Add(p);
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

        public void FindMatches() {
            for (int i = 0; i < _actions.Count; i++) {
                for (int j = i+1; j < _actions.Count; j++) {
                    if (_actions[i].CanSyncWith(_actions[j])) {
                        _matches.Add(new Match(_actions[i], _actions[j]));
                    }
                }
            }
            if (_matches.Count == 0) {
                Console.WriteLine("System is deadlocked");
                Console.ReadKey();
                Environment.Exit(1);
            }
            Match m = _matches[0];
            foreach (ProcessBase p in _activeProcs) {
                if (p.ID == m.a1.ProcessID || p.ID == m.a2.ProcessID) {
                    p.Continue();
                }
            }
        }


    }
}
