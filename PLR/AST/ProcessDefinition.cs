using System.Collections.Generic;
using PLR.AST.Processes;

namespace PLR.AST {

    public class ProcessDefinition : Node{
        public ProcessConstant ProcessConstant;// { get; set; }
        public Process Process;// { get; set; }
        public bool EntryProc;// { get; set; }

        public ProcessDefinition(ProcessConstant pconst, Process proc)
        {
            Process = proc;
            ProcessConstant = pconst;
            _children.Add(pconst);
            _children.Add(proc);
        }
    }
}
