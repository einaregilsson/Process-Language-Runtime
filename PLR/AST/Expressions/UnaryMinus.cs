using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

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
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Compile(ILGenerator il) {
            _exp.Compile(il);
            il.Emit(OpCodes.Neg);
        }
    }
}
