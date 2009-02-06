using System.Collections.Generic;


namespace PLR.AST {
    public class Restrictions : Node {
        public void Add(ActionID action)
        {
            _children.Add(action);
        }

    }
}
