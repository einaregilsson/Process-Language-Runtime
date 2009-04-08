using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST.Expressions;
using PLR.Compilation;

namespace PLR.AST {
    public class ExpressionList : Node {

        public void Add(Expression exp) {
            _children.Add(exp);
        }

        public new Expression this[int index] {
            get { return (Expression)_children[index]; }
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
        }
    }
}
