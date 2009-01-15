using System.Collections.Generic;

namespace CCS.Nodes {

    public class ActionPrefix : Process{

        public ActionPrefix(Action action) {
            this.Action = action;
        }

        public Action Action { get; private set; }
        public Process Process { get; set; }

        public override List<ASTNode> GetChildren() {
            return new List<ASTNode>(){ this.Process };
        }
    }
}
