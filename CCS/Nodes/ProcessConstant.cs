using System.Collections.Generic;

namespace CCS.Nodes {
    public class ProcessConstant : Process {
        public ProcessConstant() {
            Subscript = new Subscript();
        }
        public string Name { get; set; }
        public Subscript Subscript { get; private set; }

        public override List<ASTNode> GetChildren() {
            return new List<ASTNode>() { Subscript };
        }
    }
}
