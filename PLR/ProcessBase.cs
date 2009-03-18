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

        public ProcessBase() {
            Scheduler.Instance.AddProcess(this);
        }

        private Thread _procThread;
        public void Run() {
            Thread t = new Thread(new ThreadStart(this.StartProcess));
            t.Start();
        }

        public const int KILL_YOURSELF = -1;

        protected void InitSetID() {
            if (this.SetID == Guid.Empty) {
                this.SetID = Guid.NewGuid();
            }
        }

        public ThreadState State {
            get { return _procThread != null ? _procThread.ThreadState : ThreadState.Unstarted; }
        }

        protected void Debug(string msg) {
            lock (_colors) {
                ConsoleColor original = Console.ForegroundColor;
                if (_colors.ContainsKey(_id)) {
                    Console.ForegroundColor = _colors[_id];
                }
                Console.WriteLine(this.GetType().Name + "_" + this.ID + ": " + msg);
                Console.ForegroundColor = original;
            }
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
                Debug("Was told to kill myself");
                _procThread.Abort();
            } else {
                Debug("Executing chosen action");
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
        private static ConsoleColor[] _availableColors = new ConsoleColor[] { ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.Green, ConsoleColor.Magenta, ConsoleColor.Red, ConsoleColor.Yellow };
        private static Dictionary<int, ConsoleColor> _colors = new Dictionary<int, ConsoleColor>();
        private void StartProcess() {
            _procThread = Thread.CurrentThread;
            _id = Thread.CurrentThread.ManagedThreadId;
            lock(_colors) {
                foreach (ConsoleColor c in _availableColors) {
                    if (!_colors.ContainsValue(c)) {
                        _colors.Add(_id, c);
                        break;
                    }
                }
            }
            RunProcess();
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
