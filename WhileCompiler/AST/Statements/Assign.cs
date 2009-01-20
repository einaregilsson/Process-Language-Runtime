using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using While.AST.Expressions;

namespace While.AST.Statements {
    public class Assign : Statement {
        public Variable Variable { get; protected set; }
        public Expression Expression { get; protected set; }
        public Assign(Variable var, Expression exp) {
            this.Variable = var;
            this.Expression = exp;
        }

        public override List<Node> GetChildren() {
            return new List<Node>() { this.Variable, this.Expression };
        }
        
    }
}
