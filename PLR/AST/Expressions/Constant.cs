
namespace PLR.AST.Expressions {

    public class Constant : ArithmeticExpression{
        private int _number;
        public Constant(int number) {
            _number = number;
        }

        public override int Value {
            get { return _number; }
        }
    }
}
