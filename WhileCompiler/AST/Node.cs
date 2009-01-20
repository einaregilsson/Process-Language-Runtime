using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST {
    
    public abstract class Node {
        public virtual List<Node> GetChildren() {
            return new List<Node>();
        }
    }
}
