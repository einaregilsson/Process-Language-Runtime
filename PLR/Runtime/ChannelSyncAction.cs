/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace PLR.Runtime {

    public class ChannelSyncAction : IAction {

        private ProcessBase _proc;
        private string _name;
        private bool _input;
        private List<object> _values = new List<object>();
        private int _paramCount = 0;

        public ChannelSyncAction(string name, ProcessBase p, int paramCount, bool input) {
            _proc = p;
            _name = name;
            _input = input;
            _paramCount = paramCount;
        }

        public void AddValue(object value) {
            _values.Add(value);
        }

        public object GetValue(int index) {
            return _values[index];
        }

        public bool CanSyncWith(IAction other) {
            if (!(other is ChannelSyncAction)) {
                return false;
            }
            ChannelSyncAction otherAction = (ChannelSyncAction)other;
            return otherAction._name == this._name 
                && otherAction._input != this._input
                && otherAction._paramCount == this._paramCount;
        }

        public void Sync(IAction other) {
            if (this._input) { //Only process in the input action
                ChannelSyncAction cout = (ChannelSyncAction) other;
                foreach (int value in cout._values) {
                    this._values.Add(value);
                }
            }
            //do nothing
        }

        public override string ToString() {
            string str = _input ? _name : "_" + _name + "_";
            if (_paramCount > 0) {
                str += "[" + _paramCount + "]";
            }
            return str;
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

    }
}
