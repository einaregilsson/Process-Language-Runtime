using System.Collections.Generic;

namespace PLR.AST {
    public class Relabellings : Node {
        
        public void Add(ActionID from, ActionID to) {
            _children.Add(from);
            _children.Add(to);
        }
        public ActionID this[ActionID val] {
            get {
                for (int i = 0; i < _children.Count; i += 2) {
                    if (_children[i].Equals(val))
                    {
                        return (ActionID) _children[i + 1];
                    }
                }
                return null;
            }
        }
    }
}
