using System.Collections.Generic;

namespace PLR.AST {

    public class ProcessSystem : Node  {

        public new ProcessDefinition this[int index] {
            get { return (ProcessDefinition)_children[index]; }
        }

        public void Add(ProcessDefinition pd) {
            _children.Add(pd);
        }

    }
}
