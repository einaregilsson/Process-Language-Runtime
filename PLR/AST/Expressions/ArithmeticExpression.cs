using System;
using System.Collections.Generic;

using System.Text;

namespace PLR.AST.Expressions {
    public abstract class ArithmeticExpression : Node{
        public abstract int Value { get; }
    }
}
