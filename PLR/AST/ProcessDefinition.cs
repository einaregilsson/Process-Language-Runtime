using System.Collections.Generic;
using PLR.AST.Processes;

namespace PLR.AST {

    public class ProcessDefinition : Node{

        protected ProcessConstant _procConst;
        public ProcessConstant ProcessConstant
        {
            get { return _procConst; }
            set { _procConst = value; }
        }

        protected Process _proc;
        public Process Process
        {
            get { return _proc; }
            set { _proc = value; }
        }

        protected bool _entryProc;
        public bool EntryProc
        {
            get { return _entryProc; }
            set { _entryProc = value; }
        }


        public ProcessDefinition(ProcessConstant pconst, Process proc, bool entryProc)
        {
            _proc = proc;
            _procConst = pconst;
            _entryProc = entryProc;
            _children.Add(pconst);
            _children.Add(proc);
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }


    }
}
