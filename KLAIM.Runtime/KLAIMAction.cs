using System;
using System.Collections.Generic;
using System.Text;
using PLR.Runtime;

namespace KLAIM.Runtime {
    public class KLAIMAction : IAction{

        private ProcessBase _proc;
        private string _displayName;
        public KLAIMAction(ProcessBase p, string displayName) {
            _proc = p;
            _displayName = displayName;
        }

        private class Test {
            public object x;
        }
        public bool CanSyncWith(IAction other) {
            //Tuple t = new Tuple(new object[] { 1, 3, 4, "234" });
            //Test test = new Test();
            //test.x = t.GetValueAt(0);

            return false;
        }

        public override string ToString() {
            return _displayName;
        }

        public void Sync(IAction other) {
            //Do nothing
        }

        public int ProcessID {
            get { return _proc.ID; }
        }

        public bool IsAsynchronous {
            get { return true; }
        }

    }
}
