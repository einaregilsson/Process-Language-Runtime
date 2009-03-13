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
        private bool _input;
        public StringAction(string name, ProcessBase p, bool input) {
            _proc = p;
            _name = name;
            _input = input;
        }
        public bool CanSyncWith(IAction other) {
            if (!(other is StringAction)) {
                return false;
            }
            StringAction otherAction = (StringAction) other;
            return otherAction._name == this._name && otherAction._input != this._input;
        }
        public override string ToString() {
            return _name;
        }

        public int ProcessID {
            get { return _proc.ID;  }
        }

        public bool IsAsynchronous {
            get { return false; }
        }

        public void Execute() {
            //throw new Exception("The method or operation is not implemented.");
        }

    }
}
