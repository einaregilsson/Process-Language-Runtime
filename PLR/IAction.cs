using System;
using System.Collections.Generic;
using System.Text;

namespace PLR {
    public interface IAction {
        bool CanSyncWith(IAction other);
        int ProcessID { get; }
        bool IsAsynchronous { get; }
        void Execute();
    }

    public class StringAction : IAction {

        private ProcessBase _proc;
        private string _name;
        public StringAction(string name, ProcessBase p) {
            _proc = p;
            _name = name;
        }
        public bool CanSyncWith(IAction other) {
            return ((StringAction)other)._name == this._name;
        }

        public int ProcessID {
            get { return _proc.ID;  }
        }

        public bool IsAsynchronous {
            get { return false; }
        }

        public void Execute() {
            throw new Exception("The method or operation is not implemented.");
        }

    }
}
