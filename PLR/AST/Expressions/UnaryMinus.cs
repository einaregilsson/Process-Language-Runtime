using System;
using System.Collections.Generic;

namespace PLR.AST.Expressions {
    public class UnaryMinus : ArithmeticExpression{
        protected ArithmeticExpression _exp;
        public ArithmeticExpression Expression { get { return _exp; } }

        public UnaryMinus(ArithmeticExpression exp) {
            _exp = exp;
            _children.Add(exp);
        }
        public override int Value {
            get { return -this.Expression.Value; }
        }
    }
}
