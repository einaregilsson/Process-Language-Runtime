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
using System.Threading;

namespace PLR.Runtime {

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
    #endregion

    /// <summary>
    /// The scheduler for process execution. It selects which actions to execute and
    /// process must register and unregister with it as they enter and leave the 
    /// system.
    /// </summary>
    public class Scheduler {

        #region Constructor
        private Scheduler() {
            this.SelectAction = new SelectActionToExecute(this.ChooseActionRandom);
            foreach (string arg in Environment.GetCommandLineArgs()) {
                if (arg.ToLower() == "/i" || arg.ToLower() == "/interactive") {
                    this.SelectAction = new SelectActionToExecute(this.ChooseActionInterActive);
                }
            }
        }
        #endregion

        #region Events
        public event ProcessChangeEventHandler ProcessRegistered;
        public event ProcessChangeEventHandler ProcessKilled;
        public event TraceEventHandler TraceItemAdded;
        #endregion

        #region Static
        private static Scheduler _instance = new Scheduler();

        public static Scheduler Instance {
            get { return _instance; }
        }

        #endregion

        #region Fields
        private List<string> _trace = new List<string>();
        private Random _rng = new Random();
        private List<ProcessBase> _activeProcs = new List<ProcessBase>();
        private bool _stop = false;
        #endregion

        #region Properties
        public SelectActionToExecute SelectAction { get; set; }
        #endregion

        #region Public methods

        public void AddProcess(ProcessBase p) {
            Debug("Added " + p);
            lock (_activeProcs) {
                _activeProcs.Add(p);
            }
            if (ProcessRegistered != null) {
                ProcessRegistered(this, new ProcessEventArgs() { Process = p });
            }
        }

        //Instead of resetting all members, just create a new global
        //instance.
        public void Reset() {
            _activeProcs.Clear();
            _trace.Clear();
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

        public void StopAllProcesses() {
            _stop = true;
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
                if (allWaiting && _stop) {
                    _stop = false;
                    SuspendOrAbortProcesses();
                    lock (GlobalScope.Actions) {
                        GlobalScope.Actions.Clear();
                    }
                    return;
                } else if (allWaiting) {
                    bool stillActive = SelectAndExecuteAction();
                    if (!stillActive) {
                        SuspendOrAbortProcesses();
                        return;
                    }
                }
                Thread.Sleep(50);
            }
        }

        #endregion

        #region Private methods

        private void SuspendOrAbortProcesses() {
            lock (_activeProcs) {
                foreach (ProcessBase p in _activeProcs) {
                    if (p.Thread.ThreadState == ThreadState.Suspended) {
                        p.ChosenAction = null;
                        p.Continue();
                    } else if (p.Thread.ThreadState == ThreadState.WaitSleepJoin) {
                        p.Thread.Abort();
                    }
                }
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

        private ProcessBase GetProcessByID(int id) {
            foreach (ProcessBase p in _activeProcs) {
                if (p.ID == id) {
                    return p;
                }
            }
            throw new Exception("Found no process with id " + id);
            //return null;
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
        /// Finds all candidate actions for execution, and selects one to
        /// execute using the SelectionActionToExecute delegate.
        /// </summary>
        /// <returns>
        /// true if the Scheduler should continue, false if it is
        /// deadlocked.
        /// </returns>
        private bool SelectAndExecuteAction() {

            List<CandidateAction> candidates = FindAllCandidateActions();

            if (candidates.Count == 0) {
                return HandleNoCandidateActions();
            }

            CandidateAction chosen;
            chosen = SelectAction(candidates);

            TraceEventArgs traceArgs = new TraceEventArgs() { ExecutedAction = chosen };

            Debug("Chose action");
            _trace.Add(chosen.ToString());

            if (TraceItemAdded != null) {
                TraceItemAdded(this, traceArgs);
            }

            //Now let them sync with each other if this
            //is a synchronous actino
            if (chosen.IsSync) {
                chosen.Action1.Sync(chosen.Action2);
                chosen.Action2.Sync(chosen.Action1);
            }

            List<ProcessBase> wakeUp = new List<ProcessBase>();
            List<Guid> wakeUpGuids = new List<Guid>();

            //if (chosen.Process1 == null) {
            //    throw new Exception("IS NULL");
            //}
            chosen.Process1.ChosenAction = chosen.Action1;
            wakeUpGuids.Add(chosen.Process1.SetID);
            wakeUp.Add(chosen.Process1);
            if (!chosen.IsAsync) {
                chosen.Process2.ChosenAction = chosen.Action2;
                wakeUp.Add(chosen.Process2);
                wakeUpGuids.Add(chosen.Process2.SetID);
            }

            if (chosen.IsAsync) {
                Debug("Chose async action " + chosen + ", proc " + chosen.Process1 + ".");
            } else {
                Debug("Chose action " + chosen + ", procs " + chosen.Process1 + " and " + chosen.Process2);
            }

            Logger.TraceDebug(chosen);

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

        /// <summary>
        /// Finds all candidate actions, both those restricted to particular processes
        /// and those that aren´t restricted at all and therefore have global scope.
        /// </summary>
        /// <returns></returns>
        private List<CandidateAction> FindAllCandidateActions() {
            List<ProcessBase> finished = new List<ProcessBase>();
            List<CandidateAction> candidates = new List<CandidateAction>();

            //Start by finding all actions that were restricted and therefore
            //have process scope
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

            ///...then add the unrestricted actions
            Debug("Global scope contains " + GlobalScope.Actions.Count + " candidate actions: " + Util.Join(", ", GlobalScope.Actions));
            candidates.AddRange(FindCandidateActions(GlobalScope.Actions, null));
            return candidates;
        }

        /// <summary>
        /// Called when no candidate actions are found. Handles some circumstances that may
        /// lead to the program continuing, or prints out a deadlock message and exits.
        /// </summary>
        /// <returns></returns>
        private bool HandleNoCandidateActions() {
            //Give blocked processes a chance. This was explicitly added for the KLAIM
            //implementations, where threads may have had their actions chosen but are
            //blocked performing an action
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
                        return true; //just means we run through the FindCandidateActions phase again
                    }
                }
            }

            Debug("System is deadlocked");
            Logger.TraceDebug(CandidateAction.DEADLOCK);
            if (TraceItemAdded != null) {
                TraceItemAdded(this, new TraceEventArgs() { ExecutedAction = CandidateAction.DEADLOCK });
            }

            try {
                Console.ReadKey(); //So processes started from the GUI don't exit without people seeing
                //what happened
            } catch (Exception) {
                //Just swallow it, this fails when run from a windows application so we don't
                //want the exception to cause problems.
            }
            return false; //Not still active
        }

        /// <summary>
        /// Returns a random action from the list of candidate actions.
        /// </summary>
        /// <param name="candidates">List of candidate actions</param>
        /// <returns>The randomly chosen action to execute</returns>
        private CandidateAction ChooseActionRandom(List<CandidateAction> candidates) {
            return candidates[_rng.Next(candidates.Count)];
        }

        /// <summary>
        /// Allows the user to choose the next action to execute from the console
        /// </summary>
        /// <param name="candidates">List of candidate actions</param>
        /// <returns>The action chosen by the user</returns>
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

        #endregion
    }
}
