using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Expressions {

    public class UnaryOp : Expression{
        public const string Minus = "-";
        public const string BitNegate = "~";
        public UnaryOp(string op, Expression exp) { }
    }
}
