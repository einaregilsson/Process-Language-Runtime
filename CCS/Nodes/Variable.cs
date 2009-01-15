using System.Collections.Generic;

namespace CCS.Nodes {
    public class Variable : ArithmeticExpression {

        public string Name { get; private set; }
        public Variable(string name) {
            Name = name;
        }
        public override int Value {
            get { return 2308; }
        }
    }
}
