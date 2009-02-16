using System;
using System.Collections.Generic;

using System.Text;
using PLR.AST.Expressions;

namespace PLR.AST {
    public class Subscript : Node {

        public void Add(ArithmeticExpression exp) {
            _children.Add(exp);
        }

        public new ArithmeticExpression this[int index] {
            get { return (ArithmeticExpression)_children[index]; }
        }

        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
