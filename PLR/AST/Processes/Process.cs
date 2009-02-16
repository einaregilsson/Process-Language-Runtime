
using PLR.AST;

namespace PLR.AST.Processes {
    public abstract class Process : Node {
        private Relabellings _relabelling = new Relabellings();
        public Relabellings Relabelling { get { return _relabelling; } }
        private Restrictions _restrictions = new Restrictions();
        public Restrictions Restrictions { get { return _restrictions; } }
    }
}
