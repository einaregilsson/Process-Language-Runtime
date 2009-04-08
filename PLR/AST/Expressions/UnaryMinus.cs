using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Expressions {

    public class UnaryMinus : ArithmeticExpression{
        protected Expression _exp;
        public Expression Expression { get { return _exp; } }

        public UnaryMinus(Expression exp) {
            _exp = exp;
            _children.Add(exp);
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
        
        public override string ToString() {
            return "-" + _exp.ToString();
        }

        public override void Compile(CompileContext context) {
            _exp.Compile(context);
            context.ILGenerator.Emit(OpCodes.Neg);
        }
    }
}
