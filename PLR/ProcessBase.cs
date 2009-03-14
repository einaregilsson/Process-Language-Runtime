using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using PLR.AST.Actions;

namespace PLR {

    public abstract class ProcessBase {

        private int _id;
        public int ID { get { return _id; } }

        private Guid _setID = Guid.Empty;
        public Guid SetID{ 
            get { return _setID; }
            set { _setID = value; }
        }

        private int _chosenAction;
        public int ChosenAction {
            get { return _chosenAction; }
            set { _chosenAction = value; }
        }

        private Thread _procThread;
        public void Run() {
            Thread t = new Thread(new ThreadStart(this.StartProcess));
            t.Start();
        }

        public const int KILL_YOURSELF = -1;

        public bool Waiting {
            get { return _procThread != null && _procThread.ThreadState == ThreadState.Suspended; }
        }

        protected void Debug(string msg, params object[] args) {
            Console.WriteLine(this.GetType().Name + "_" + this.ID + ": " + string.Format(msg, args));
        }
        public ThreadState state { get { return _procThread.ThreadState; } }
        protected int Sync(List<IAction> actions) {

            try {
                 Debug("Synced actions, going to sleep");
                 Scheduler.Instance.SyncActions(this, actions);
                 _procThread.Suspend();
            } catch (ThreadInterruptedException) {
                Debug("Got ex");
            }
            Debug("Woke up");
            if (this.ChosenAction == KILL_YOURSELF) {
                _procThread.Abort();
            } else {
                actions[this.ChosenAction].Execute();
            }
            return this.ChosenAction;

        }

        protected int NonDeterministicChoiceSync(List<IAction> actions) {
            return Sync(actions);
        }

        public override string ToString() {
            return this.GetType().Name + "_" + this.ID;
        }

        public void Continue() {
            _procThread.Resume();
        }

        private void StartProcess() {
            _procThread = Thread.CurrentThread;
            _id = Thread.CurrentThread.ManagedThreadId;
            RunProcess();
        }
        public abstract void RunProcess();
    }
    public class ProcA : ProcessBase {
        public override void RunProcess() {
            try {
                Scheduler.Instance.AddProcess(this);
                List<IAction> tmp = new List<IAction>();
                tmp.Add(new StringAction("a", this, true));
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
