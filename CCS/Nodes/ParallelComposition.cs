using System.Collections.Generic;

namespace CCS.Nodes {
    public class ParallelComposition : Process, IEnumerable<Process>{
        private List<Process> parallel = new List<Process>();

        public IEnumerator<Process> GetEnumerator() {
            return parallel.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return parallel.GetEnumerator();
        }

        public Process this[int index] {
            get { return parallel[index]; }
        }
        
        public int Count {
            get { return parallel.Count; }
        }

        public void Add(Process p) {
            parallel.Add(p);
        }
        public override List<ASTNode> GetChildren() {
            var l = new List<ASTNode>();
            foreach (ASTNode a in this) {
                l.Add(a);
            }
            l.Add(this.Relabelling);
            l.Add(this.Restrictions);
            return l;
        }

    }
}
