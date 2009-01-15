using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CCS.Nodes {
    public abstract class ArithmeticExpression : ASTNode{

        public abstract int Value { get; }
    }
}
