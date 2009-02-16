using System.Collections.Generic;
using PLR.AST.Actions;

namespace PLR.AST.Processes {

    public class ActionPrefix : Process{

        public ActionPrefix(Action action, Process proc) {
            _action = action;
            _proc = proc;
            _children.Add(Action);
            _children.Add(proc);
        }

        private Action _action;
        public Action Action
        {
            get { return _action; }
            set { _action = value; _children[0] = _action; }
        }

        private Process _proc;
        public Process Process {
            get { return _proc; }
            set { _proc = value; _children[1] = _proc; }
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
