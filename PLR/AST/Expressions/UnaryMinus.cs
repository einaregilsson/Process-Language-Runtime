using System;
using System.Collections.Generic;

namespace PLR.AST.Expressions {
    public class UnaryMinus : ArithmeticExpression{
        public ArithmeticExpression Expression;// { get; private set; }

        public UnaryMinus(ArithmeticExpression exp) {
            this.Expression = exp;
            _children.Add(exp);
        }
        public override int Value {
            get { return -this.Expression.Value; }
        }
    }
}
