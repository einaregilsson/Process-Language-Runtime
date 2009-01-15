using System.Collections.Generic;

namespace CCS.Nodes {
    public class ProcessDefinition : ASTNode{
        public ProcessConstant ProcessConstant { get; set; }
        public Process Process { get; set; }
        public bool EntryProc { get; set; }
        public override List<ASTNode> GetChildren() {
            return new List<ASTNode>() { ProcessConstant, Process };
        }

    }
}
