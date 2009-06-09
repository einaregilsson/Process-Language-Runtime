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

    public delegate IAction PreProcessAction(IAction action);
    public delegate bool RestrictAction(IAction action);

    public abstract class ProcessBase {

        #region Members and Properties
        private int _id;
        public int ID { get { return _id; } }

        private Guid _setID = Guid.Empty;
        public Guid SetID{ 
            get { return _setID; }
            set { _setID = value; }
        }

        private List<ProcessBase> _subProcs = new List<ProcessBase>();
        public List<ProcessBase> SubProcesses 
        {
            get { return _subProcs; }
        }

        private IAction _chosenAction;
        public IAction ChosenAction {
            get { return _chosenAction; }
            set { _chosenAction = value; }
        }

        private Thread _procThread;
        public Thread Thread {
            get { return _procThread; }
        }

        public ThreadState State {
            get { return _procThread != null ? _procThread.ThreadState : ThreadState.Unstarted; }
        }

        #endregion


        public ProcessBase() {
            Scheduler.Instance.AddProcess(this);
        }

        public void Run() {
            Thread t = new Thread(new ThreadStart(this.StartProcess));
            t.Start();
        }

        private ProcessBase _parent;
        public ProcessBase Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        public virtual PreProcessAction PreProcess {
            get {
                return delegate(IAction action) { return action; };
            }
        }

        public virtual RestrictAction Restrict {
            get { return delegate(IAction a) { return false; }; }
        }

        protected void InitSetID() {
            if (this.SetID == Guid.Empty) {
                this.SetID = Guid.NewGuid();
            }
        }

        protected void Debug(string msg) {
            Logger.ProcessDebug(msg);
        }

        private List<IAction> _localActions = new List<IAction>();
        public List<IAction> LocalActions {
            get { return _localActions; }
        }

        protected void Sync(IAction action) {
            SyncAction(action);
            Debug("Synced actions, going to sleep");
            this.Thread.Suspend();
            Debug("Woke up");
            if (this.ChosenAction == null) {
                Debug("I wasn't chosen...");
                throw new ProcessKilledException();
            } else {
                CleanActions();
                Debug("Executing chosen action");
            }
        }

        protected void SyncAction(IAction action) {
            //Relabellings etc...

            if (this.PreProcess == null) {
                Console.WriteLine("WAHT IS HAPPENS?");
            }
            action = PreProcess(action);

            //If restricted then we store it as a local action
            if (Restrict(action)) {
                _localActions.Add(action);
            } else if (Parent == null) { //Top level process, add to global scope
                Debug("Adding action from process " + action.ProcessID + " to global scope");
                GlobalScope.Actions.Add(action);
            } else {
                Parent.SyncAction(action);
            }
        }

        public override string ToString() {
            return this.GetType().FullName + "_" + this.ID;
        }

        public void Continue() {
            _procThread.Resume();
        }
        
        [System.Diagnostics.DebuggerStepThrough]
        private void StartProcess() {
            _procThread = Thread.CurrentThread;
            _id = _procThread.ManagedThreadId;
            Logger.Register(this.GetType().FullName + "_" + _id +": ");
            Debug("Started, my parent is " + this.Parent);
            RunProcess();
        }

        protected void Die() {
            CleanActions();
            Debug("Goodbye cruel world...");
            Logger.Unregister();
            Scheduler.Instance.KillProcess(this);
        }

        protected void CleanActions() {
            //Kill our candidate actions
            for (ProcessBase parent = this; parent != null; parent = parent.Parent) {
                lock (parent.LocalActions) {
                    parent.LocalActions.RemoveAll(delegate(IAction act) {
                        return act.ProcessID == this.ID;
                    });
                }
            }
            lock (GlobalScope.Actions) {
                GlobalScope.Actions.RemoveAll(delegate(IAction act) {
                    return act.ProcessID == this.ID;
                });
            }
        }

        public abstract void RunProcess();
    }
}
