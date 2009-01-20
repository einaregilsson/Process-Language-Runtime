using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Expressions {

    public class UnaryOp : Expression{
        public const string Minus = "-";
        public const string BitNegate = "~";

        private Expression expression;
        private string op;

        public UnaryOp(string op, Expression exp) {
            this.op = op;
            this.expression = exp;
        }

        public override List<Node> GetChildren() {
            return new List<Node>() { this.expression };
        }
    }
}
