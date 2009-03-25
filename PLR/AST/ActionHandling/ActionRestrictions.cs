using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;


namespace PLR.AST.ActionHandling {
    public abstract class ActionRestrictions : Node {
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
