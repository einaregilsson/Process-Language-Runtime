using System.Collections.Generic;


namespace PLR.AST {
    public class Restrictions : Node {
        public void Add(ActionID action)
        {
            _children.Add(action);
        }

        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
