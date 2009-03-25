using System.Collections.Generic;

namespace PLR.AST.ActionHandling {
    public abstract class PreProcessActions : Node {

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
