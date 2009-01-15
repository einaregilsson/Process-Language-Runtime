
namespace CCS.Nodes {
    public abstract class Process : ASTNode {
        private Relabellings relabelling = new Relabellings();
        public Relabellings Relabelling { get { return relabelling; } }
        private Restrictions restrictions = new Restrictions();
        public Restrictions Restrictions { get { return restrictions; } }
    }
}
