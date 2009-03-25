using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using PLR.AST.Actions;

namespace PLR {

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

        public virtual bool IsRestricted(IAction action) {
            if (!(action is ChannelSync)) {
                return false;
            }
            ChannelSync c = (ChannelSync)action;
            return c.Name == "A" || c.Name == "B" || c.Name == "D";
        }

        public virtual IAction PreProcess(IAction action) {
            return action;
        }

        public const int KILL_YOURSELF = -1;

        protected void InitSetID() {
            if (this.SetID == Guid.Empty) {
                this.SetID = Guid.NewGuid();
            }
        }


        protected void Debug(string msg) {
            Logger.Debug(msg);
        }

        private List<IAction> _localActions = new List<IAction>();
        public List<IAction> LocalActions {
            get { return _localActions; }
        }

        protected void Sync(List<IAction> actions) {
            SyncActions(actions);
            Debug("Synced actions, going to sleep");
            this.Thread.Suspend();
            Debug("Woke up");
            if (this.ChosenAction == null) {
                Debug("I wasn't chosen...");
                throw new ProcessKilledException();
            } else {
                Debug("Executing chosen action");
                this.ChosenAction.Execute();
                CleanActions();
            }
        }

        protected void SyncActions(List<IAction> actions) {
            //Relabellings etc...
            for (int i = 0; i < actions.Count; i++) {
                actions[i] = PreProcess(actions[i]);
            }

            //If restricted then we store it as a local action
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (IsRestricted(actions[i])) {
                    _localActions.Add(actions[i]);
                    actions.RemoveAt(i);
                }
            }

            if (Parent == null) { //Top level process, add to global scope
                foreach (IAction a in actions) {
                    Debug("Adding actions from process " + a.ProcessID + " to global scope");
                }
                GlobalScope.Actions.AddRange(actions);
            } else if (actions.Count > 0) {
                Parent.SyncActions(actions);
            }
        }

        protected void NonDeterministicChoiceSync(List<IAction> actions) {
            Sync(actions);
        }

        public override string ToString() {
            return this.GetType().Name + "_" + this.ID;
        }

        public void Continue() {
            _procThread.Resume();
        }
        
        private void StartProcess() {
            _procThread = Thread.CurrentThread;
            _id = _procThread.ManagedThreadId;
            Logger.Register(this.GetType().Name + "_" + _id +": ");
            Debug("Started");
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

    public class ProcA : ProcessBase {
        public override void RunProcess() {
            if (this.SetID == Guid.Empty) {
                this.SetID = Guid.NewGuid();
            }
            try {
                Scheduler.Instance.AddProcess(this);
                List<IAction> tmp = new List<IAction>();
                tmp.Add(new ChannelSync("a", this, true));
                this.Sync(tmp);
                //this.Sync(new StringAction("b", this, false));
                //this.Sync(new StringAction("a", this));

            } catch (ThreadAbortException ex) {
                Debug("Am being killed");
                Debug(ex.Message);
                Scheduler.Instance.KillProcess(this);
                return;
            }
            Debug("End of life for me, have turned into 0");
            Scheduler.Instance.KillProcess(this);
        }
    }

}
