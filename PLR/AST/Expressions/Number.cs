using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Expressions {

    public class Number : ArithmeticExpression{
        private int _number;
        public Number(int number) {
            _number = number;
        }

        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Print(string s) { }
        public void junk() {
            Print("Printing string");
            Print("Print expr: " + (3 + 2 / 4));
        }
        public int Value {
            get { return _number; }
        }
        public override string ToString() {
            return _number.ToString();
        }

        public override void Compile(CompileContext context) {
            context.ILGenerator.Emit(OpCodes.Ldc_I4, _number);
        }
    }
}
