using System;
using System.Collections.Generic;

namespace PLR.AST.Expressions {
    public class Variable : ArithmeticExpression {
        protected string _name;
        public string Name { get { return _name; } }
        public Variable(string name) {
            _name = name;
        }
        public override int Value {
            get { throw new NotSupportedException("Variable value not available at compile time!"); }
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

    }
}
