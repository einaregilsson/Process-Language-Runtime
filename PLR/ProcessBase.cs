using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using PLR.AST.Actions;

namespace PLR {
    public abstract class ProcessBase {

        private int _id;
        public int ID { get { return _id; } }
        public static Scheduler _scheduler = new Scheduler();
        private Thread _procThread;
        public void Run() {
            Thread t = new Thread(new ThreadStart(this.StartProcess));
            t.Start();
        }

        public bool Waiting {
            get { return _procThread != null && _procThread.ThreadState == ThreadState.Suspended; }
        }

        public ThreadState state { get { return _procThread.ThreadState; } }
        protected void Sync(params IAction[] actions) {

            try {
                _scheduler.SyncActions(actions);
                Console.WriteLine("Synced actions, going to sleep");
                _procThread.Suspend();
            } catch (ThreadInterruptedException) {
                string s = "234";
                Console.WriteLine("Got ex");
            }
            Console.WriteLine("Woke up");
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
            _scheduler.AddProcess(this);
            this.Sync(new StringAction("a", this), new StringAction("b", this));
            this.Sync(new StringAction("c", this), new StringAction("d", this));
            Console.WriteLine("Killed myself");
            _scheduler.KillProcess(this);
        }
    }

    public class ProcB : ProcessBase {
        public override void Start() {
            _scheduler.AddProcess(this);
            this.Sync(new StringAction("a", this), new StringAction("b", this));
            this.Sync(new StringAction("a", this), new StringAction("b", this));
            Console.WriteLine("Killed myself");
            _scheduler.KillProcess(this);
        }
    }

}
