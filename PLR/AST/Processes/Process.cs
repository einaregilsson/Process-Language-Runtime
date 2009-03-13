
using PLR.AST;
using PLR.AST.Actions;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PLR.AST.Processes {
    public abstract class Process : Node {
        private Relabellings _relabelling = new Relabellings();
        public Relabellings Relabelling { get { return _relabelling; } }
        private Restrictions _restrictions = new Restrictions();
        public Restrictions Restrictions { get { return _restrictions; } }

        public abstract void Compile(ILGenerator il);
    }
}
