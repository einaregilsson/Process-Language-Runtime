using System;
using System.Collections.Generic;
using System.Text;

namespace PLR {
    public class MethodCallAction : IAction {

        private ProcessBase _proc;
        private string _displayName;

        public MethodCallAction(string name, ProcessBase p) {
            _proc = p;
            _displayName = name;
        }

        public bool CanSyncWith(IAction other) {
            return false; //Method calls don't sync with anything
        }

        public int ProcessID {
            get { return _proc.ID; }
        }

        public override string ToString() {
            return _displayName;
        }

        public bool IsAsynchronous {
            get { return true; } //Meaning that you don't sync with another action
        }
    }
}
