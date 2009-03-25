using System;
using System.Collections.Generic;
using System.Text;

namespace PLR {

    public class ChannelSync : IAction {

        private ProcessBase _proc;
        private string _name;
        private bool _input;
        public ChannelSync(string name, ProcessBase p, bool input) {
            _proc = p;
            _name = name;
            _input = input;
        }
        public bool CanSyncWith(IAction other) {
            if (!(other is ChannelSync)) {
                return false;
            }
            ChannelSync otherAction = (ChannelSync)other;
            return otherAction._name == this._name && otherAction._input != this._input;
        }
        public override string ToString() {
            return _input ? _name : "_" + _name + "_";
        }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }
        public int ProcessID {
            get { return _proc.ID; }
        }

        public bool IsAsynchronous {
            get { return false; }
        }

        public void Execute() {
            //throw new Exception("The method or operation is not implemented.");
        }

    }
}
