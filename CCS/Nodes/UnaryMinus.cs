using System;
using System.Collections.Generic;

namespace CCS.Nodes {
    public class UnaryMinus : ArithmeticExpression{
        public ArithmeticExpression Term { get; private set; }

        public UnaryMinus(ArithmeticExpression term) {
            this.Term = term;
        }
        public override int Value {
            get { return -this.Term.Value; }
        }

        public override List<ASTNode> GetChildren() {
            return new List<ASTNode>() { Term };
        }
    }
}
