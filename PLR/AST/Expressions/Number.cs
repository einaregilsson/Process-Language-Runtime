using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {

    public class Number : ArithmeticExpression{
        private int _number;
        public Number(int number) {
            _number = number;
        }

        public override int Value {
            get { return _number; }
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override void Compile(CompileContext context) {
            context.ILGenerator.Emit(OpCodes.Ldc_I4, _number);
        }
    }
}
