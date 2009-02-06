
using PLR.AST;

namespace PLR.AST.Processes {
    public abstract class Process : Node {
        private Relabellings relabelling = new Relabellings();
        public Relabellings Relabelling { get { return relabelling; } }
        private Restrictions restrictions = new Restrictions();
        public Restrictions Restrictions { get { return restrictions; } }
    }
}
