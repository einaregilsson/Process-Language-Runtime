using System.Collections.Generic;
using PLR.AST.Actions;

namespace PLR.AST.Processes {

    public class ActionPrefix : Process{

        public ActionPrefix(Action action, Process proc) {
            this.Action = action;
            this.Process = proc;
            _children.Add(Action);
            _children.Add(proc);
        }

        public Action Action;// { get; private set; }
        public Process Process;// { get; set; }
    }
}
