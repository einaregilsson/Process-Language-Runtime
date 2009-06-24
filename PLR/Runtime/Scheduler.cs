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
    public delegate CandidateAction SelectActionToExecute(List<CandidateAction> candidates);
    public class ProcessEventArgs {
        public ProcessBase Process { get; set; }
    }

    public class TraceEventArgs {
        public CandidateAction ExecutedAction { get; set; }
    }

    public class CandidateActionsEventArgs {
        public List<CandidateAction> CandidateActions { get; set; }
    }

    public class CandidateAction {
        public IAction Action1 { get; set; }
        public IAction Action2 { get; set; }
        public ProcessBase Process1 { get; set; }
        public ProcessBase Process2 { get; set; }
        public ProcessBase ScopeProcess { get; set; }
        public static readonly CandidateAction DEADLOCK = new CandidateAction(null, null, null, null, null);

        public bool IsDeadlock {
            get {
                return this == CandidateAction.DEADLOCK;
            }
        }

        public bool IsTau {
            get {
                return ScopeProcess != null;
            }
        }

        public bool IsAsync {
            get {
                return Action1.IsAsynchronous;
            }
        }

        public bool IsSync {
            get {
                return Action1 != null && Action2 != null;
            }
        }

        
        public CandidateAction(IAction a1, IAction a2, ProcessBase p1, ProcessBase p2, ProcessBase scopeProcess) {
            Action1 = a1;
            Action2 = a2;
            Process1 = p1;
            Process2 = p2;
            ScopeProcess = scopeProcess;
        }

        public override string ToString() {
            return Action1.ToString();
        }
    }

    #endregion


    public class Scheduler {

        private Scheduler() {
            this.SelectAction = new SelectActionToExecute(this.ChooseActionInterActive);
            foreach (string arg in Environment.GetCommandLineArgs()) {
                if (arg.ToLower() == "/i" || arg.ToLower() == "/interactive") {
                    this.Interactive = true;
                    this.SelectAction = new SelectActionToExecute(this.ChooseActionInterActive);
                }
            }
        }

        public event ProcessChangeEventHandler ProcessRegistered;
        public event ProcessChangeEventHandler ProcessKilled;
        public event TraceEventHandler TraceItemAdded;

        private List<string> _trace = new List<string>();
        private Random _rng = new Random();
        private static Scheduler _instance = new Scheduler();
        public SelectActionToExecute SelectAction { get; set; }
        

        //Instead of resetting all members, just create a new global
        //instance.
        public static void Reset() {
            //Keep the selection method
            SelectActionToExecute tmp = _instance.SelectAction;
            _instance = new Scheduler();
            _instance.SelectAction = tmp;
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
                    bool stillActive = FindCandidateActions();
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

        private ProcessBase GetProcessByID(int id) {
            foreach (ProcessBase p in _activeProcs) {
                if (p.ID == id) {
                    return p;
                }
            }
            return null;
        }
        private List<CandidateAction> FindCandidateActions(List<IAction> actions, ProcessBase scope) {
            List<CandidateAction> candidates = new List<CandidateAction>();
            for (int i = 0; i < actions.Count; i++) {
                if (actions[i].IsAsynchronous) {
                    candidates.Add(new CandidateAction(actions[i], null, GetProcessByID(actions[i].ProcessID), null, scope));
                } else {
                    for (int j = i + 1; j < actions.Count; j++) {
                        int p1id = actions[i].ProcessID, 
                            p2id = actions[j].ProcessID;
                        Guid p1setID = ProcessIdToSetId(p1id), p2setID = ProcessIdToSetId(p2id);
                        if (actions[i].CanSyncWith(actions[j])
                            && (p1setID != p2setID || p1setID == Guid.Empty)) {
                            candidates.Add(new CandidateAction(actions[i], actions[j], GetProcessByID(p1id), GetProcessByID(p2id), scope));
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
        public bool FindCandidateActions() {
            List<ProcessBase> finished = new List<ProcessBase>();
            List<CandidateAction> candidates = new List<CandidateAction>();


            lock (_activeProcs) {
                foreach (ProcessBase proc in _activeProcs) {
                    ProcessBase parent = proc;
                    while (parent != null) {
                        if (!finished.Contains(parent)) {
                            Debug(parent + " contains " + parent.LocalActions.Count + " candidate actions: " + Util.Join(", ", parent.LocalActions));
                            candidates.AddRange(FindCandidateActions(parent.LocalActions, parent));
                            finished.Add(parent);
                        }
                        parent = parent.Parent;
                    }
                }
            }

            Debug("Global scope contains " + GlobalScope.Actions.Count + " candidate actions: " + Util.Join(", ", GlobalScope.Actions));
            candidates.AddRange(FindCandidateActions(GlobalScope.Actions, null));

            if (candidates.Count == 0) {
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
                    TraceItemAdded(this, new TraceEventArgs() { ExecutedAction = null });
                }
                try {
                    Console.ReadKey();
                } catch (Exception ex) {
                    //Just swallow it, this fails when run from a windows application so we don't
                    //want the exception to cause problems.
                }
                return false; //Not still active
            }

            CandidateAction chosen;
            chosen = SelectAction(candidates);

            TraceEventArgs traceArgs = new TraceEventArgs() { ExecutedAction = chosen };

            Debug("Chose action");
            if (chosen.IsTau) {
                _trace.Add("t - " + chosen);
            } else {
                _trace.Add(chosen.ToString());
            }

            if (TraceItemAdded != null) {
                TraceItemAdded(this, traceArgs);
            }

            //Now let them sync with each other
            if (!chosen.IsAsync) {
                chosen.Action1.Sync(chosen.Action2);
                chosen.Action2.Sync(chosen.Action1);
            }

            List<ProcessBase> wakeUp = new List<ProcessBase>();
            List<Guid> wakeUpGuids = new List<Guid>();
            chosen.Process1.ChosenAction = chosen.Action1;
            wakeUpGuids.Add(chosen.Process1.SetID);
            if (!chosen.IsAsync) {
                chosen.Process2.ChosenAction = chosen.Action2;
                wakeUpGuids.Add(chosen.Process2.SetID);
            }

            if (chosen.IsAsync) {
                Debug("Chose async action " + chosen + ", proc " + chosen.Process1 + ".");
            } else {
                Debug("Chose action " + chosen + ", procs " + chosen.Process1 + " and " + chosen.Process2);
            }

            if (chosen.IsAsync) {
                Logger.TraceDebug(chosen.ToString(), TraceType.MethodCall);
            } else if (chosen.IsTau) {
                Logger.TraceDebug(chosen.ToString(), TraceType.Tau);
            } else {
                Logger.TraceDebug(chosen.ToString(), TraceType.Sync);
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

        private CandidateAction ChooseActionRandom(List<CandidateAction> candidates) {
            return candidates[_rng.Next(candidates.Count)];
        }

        private CandidateAction ChooseActionInterActive(List<CandidateAction> candidates) {

            int choice = -1;
            Console.WriteLine("\nInteractive mode: ");
            for (int i = 0; i < candidates.Count; i++) {
                Console.WriteLine((i + 1) + ". " + candidates[i]);
            }
            Console.WriteLine();
            while (choice < 1 || choice > candidates.Count) {
                Console.Write("Type the number of the next action to perform: ");
                string input = Console.ReadLine();
                if (!int.TryParse(input, out choice)) {
                    choice = -1;
                }
            }
            return candidates[choice - 1];
        }
    }
}
