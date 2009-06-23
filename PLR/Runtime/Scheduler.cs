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

    public enum TraceType {
        Sync,
        Tau,
        MethodCall,
        Deadlock
    }

    #region Event infrastructure
    public delegate void ProcessChangeEventHandler(object sender, ProcessEventArgs e);
    public delegate void TraceEventHandler(object sender, TraceEventArgs e);
    public class ProcessEventArgs {
        public ProcessBase Process { get; set; }
    }

    public class TraceEventArgs {
        public int ProcessID1 { get; set; }
        public int ProcessID2 { get; set; }
        public IAction Action1 { get; set; }
        public IAction Action2 { get; set; }
        public TraceType Type { get; set; }
        public string ItemAsString { get; set; }
    }
    #endregion


    public class Scheduler {

        private Scheduler() {
            foreach (string arg in Environment.GetCommandLineArgs()) {
                if (arg.ToLower() == "/i" || arg.ToLower() == "/interactive") {
                    this.Interactive = true;
                }
            }
        }

        public event ProcessChangeEventHandler ProcessRegistered;
        public event ProcessChangeEventHandler ProcessKilled;
        public event TraceEventHandler TraceItemAdded;

        private List<string> _trace = new List<string>();
        private static Scheduler _instance = new Scheduler();
        
        //Instead of resetting all members, just create a new global
        //instance.
        public static void Reset() {
            _instance = new Scheduler();
        }

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
            if (ProcessRegistered != null) {
                ProcessRegistered(this, new ProcessEventArgs() { Process = p });
            }
        }

        public void KillProcess(ProcessBase p) {
            lock (_activeProcs) {
                _activeProcs.Remove(p);
                Debug("Killed " + p);
            }
            if (ProcessKilled != null) {
                ProcessKilled(this, new ProcessEventArgs() { Process = p });
            }

        }

        private class Match {
            public IAction a1, a2;
            public bool IsTau { get; set; }
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

                if (StepMode) {
                    while (!MayDoNextStep) {
                        Thread.Sleep(200);
                    }
                    MayDoNextStep = false;
                }

                Debug("Procs: " + _activeProcs.Count);
                bool allWaiting = true;
                lock (_activeProcs) {
                    foreach (ProcessBase p in _activeProcs) {
                        allWaiting &= (p.State == ThreadState.Suspended || p.State == ThreadState.WaitSleepJoin);
                        Debug(p + " is in state: " + p.State);
                    }
                }
                if (allWaiting) {
                    bool stillActive = FindMatches();
                    if (!stillActive) {
                        return;
                    }
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        
        public bool MayDoNextStep { get; set; }
        public bool StepMode { get; set; }

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
                        int p1id = actions[i].ProcessID, 
                            p2id = actions[j].ProcessID;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the Scheduler should continue, false if it is
        /// deadlocked.</returns>
        public bool FindMatches() {
            List<ProcessBase> finished = new List<ProcessBase>();
            List<Match> matches = new List<Match>();


            lock (_activeProcs) {
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
            }

            foreach (Match restrictedMatch in matches) {
                restrictedMatch.IsTau = true; //Everything we have looked at so far has been restricted
                                //Only the actions from the global scope are outside observable
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
                            return true;
                        }
                    }
                }

                Debug("System is deadlocked");
                Logger.TraceDebug("<DEADLOCKED>", TraceType.Deadlock);
                if (TraceItemAdded != null) {
                    TraceItemAdded(this, new TraceEventArgs() { Type = TraceType.Deadlock, ItemAsString="<DEADLOCK>" });
                }
                try {
                    Console.ReadKey();
                } catch (Exception ex) {
                    //Just swallow it, this fails when run from a windows application so we don't
                    //want the exception to cause problems.
                }
                return false; //Not still active
            }

            Match m;
            if (Interactive) {
                m = ChooseMatchInterActive(matches);
            } else {
                m = matches[new Random().Next(matches.Count)];
            }

            TraceEventArgs traceArgs = new TraceEventArgs();
            traceArgs.ProcessID1 = m.a1.ProcessID;
            traceArgs.Action1 = m.a1;
            traceArgs.Action2 = m.a2;
            traceArgs.Type = TraceType.Sync;

            Debug("Chose match");
            if (m.IsTau) {
                _trace.Add("t - (" + m.a1.ToString().Replace("_","") + ")");
                traceArgs.Type = TraceType.Tau;
            } else {
                _trace.Add(m.a1.ToString().Replace("_", ""));
            }

            if (m.a1.IsAsynchronous) {
                traceArgs.Type = TraceType.MethodCall;
            }
            traceArgs.ItemAsString = _trace[_trace.Count - 1];

            if (TraceItemAdded != null) {
                TraceItemAdded(this, traceArgs);
            }

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

            if (m.a2 == null) {
                Logger.TraceDebug(_trace[_trace.Count - 1], TraceType.MethodCall);
            } else if (m.IsTau) {
                Logger.TraceDebug(_trace[_trace.Count - 1], TraceType.Tau);
            } else {
                Logger.TraceDebug(_trace[_trace.Count - 1], TraceType.Sync);
            }


            Debug("TRACE: " + "\n     " + String.Join("\n     ", _trace.ToArray()));
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
            return true;
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
