
namespace CCS.Nodes {
    public class Constant : ArithmeticExpression{
        private int number;
        public Constant(int number) {
            this.number = number;
        }

        public override int Value {
            get { return this.number; }
        }
    }
}
