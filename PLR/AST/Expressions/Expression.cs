using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {
    public abstract class Expression : Node{
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }
        public abstract Type Type { get; }
        public abstract void Compile(ILGenerator il);
    }
}
