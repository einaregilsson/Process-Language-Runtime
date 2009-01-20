using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Expressions {
    public abstract class ArithmeticExpression : Expression {
        public abstract int IntValue { get; protected set; }
        public override object Value {
            get { return this.IntValue; }
        }
    }
}
