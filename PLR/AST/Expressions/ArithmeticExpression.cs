using System;
using System.Collections.Generic;

using System.Text;

namespace PLR.AST.Expressions {
    public abstract class ArithmeticExpression : Expression{
        public abstract int Value { get; }
        public override Type Type {
            get { return typeof(int); }
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
