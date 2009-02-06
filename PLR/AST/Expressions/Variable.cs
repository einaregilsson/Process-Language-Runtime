using System;
using System.Collections.Generic;

namespace PLR.AST.Expressions {
    public class Variable : ArithmeticExpression {

        public string Name;// { get; private set; }
        public Variable(string name) {
            Name = name;
        }
        public override int Value {
            get { throw new NotSupportedException("Variable value not available at compile time!"); }
        }
    }
}
