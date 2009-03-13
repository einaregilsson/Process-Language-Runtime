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

        public static Scheduler _scheduler = new Scheduler();
        
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
        protected int Sync(params IAction[] actions) {

            try {
                 Debug("Synced actions, going to sleep");
                 _scheduler.SyncActions(this, actions);
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

        protected int NonDeterministicChoiceSync(params IAction[] actions) {
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
            Start();
        }
        public abstract void Start();
    }
    public class ProcA : ProcessBase {
        public override void Start() {
            try {
                _scheduler.AddProcess(this);

                this.Sync(new StringAction("a", this, true));
                this.Sync(new StringAction("b", this, false));
                //this.Sync(new StringAction("a", this));

            } catch (ThreadAbortException ex) {
                Debug("Am being killed");
                Debug(ex.Message);
                _scheduler.KillProcess(this);
                return;
            }
            Debug("End of life for me, have turned into 0");
            _scheduler.KillProcess(this);
        }
    }

    public class ProcView : ProcessBase {
        public override void Start() {
            try {
                _scheduler.AddProcess(this);

                this.Sync(new StringAction("a", this,true));
                this.Sync(new StringAction("b", this,false));
                //this.Sync(new StringAction("a", this));

            } catch (ThreadAbortException ex) {
                Debug("Am being killed");
                Debug(ex.Message);
                _scheduler.KillProcess(this);
                return;
            }
            Debug("End of life for me, have turned into 0");
            _scheduler.KillProcess(this);
        }
    }

    public class ProcB : ProcessBase {
        public override void Start() {
            try {
                if (SetID == Guid.Empty) {
                    SetID = Guid.NewGuid();
                }
                Debug("I'M ALIVE!!!");
                _scheduler.AddProcess(this);

                // a.b.(a.0+b.0+B)

                //Step 1
                //this.Sync(new StringAction("a", this));

                //Step 2
                //this.Sync(new StringAction("b", this));

                //Create candidate proc
                ProcB newb = new ProcB();
                newb.SetID = this.SetID;
                newb.Run();

                //Non-deterministic Choice
                //IAction[] choices = new IAction[] { new StringAction("a", this), new StringAction("b", this) };
                int chosen =1;// = this.NonDeterministicChoiceSync(choices);
                Debug("Chose " + chosen);
                if (chosen == 0) {
                    //...continue after "a"
                } else if (chosen == 1) {
                    ProcB morph = new ProcB();
                    morph.SetID = this.SetID;
                    Debug("Morphed into new ProcB");
                    morph.Run();
                    _scheduler.KillProcess(this);
                    return;
                }
            } catch (ThreadAbortException ex) {
                Debug("Am being killed");
                _scheduler.KillProcess(this);
                return;
            }
            _scheduler.KillProcess(this);
            Debug("End of life for me, have turned into 0");
        }
    }

}
