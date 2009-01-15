using System.Collections.Generic;

namespace CCS.Nodes {

    public class NonDeterministicChoice : Process, IEnumerable<Process> {
        
        private List<Process> choices = new List<Process>();

        public IEnumerator<Process> GetEnumerator() {
            return choices.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return choices.GetEnumerator();
        }

        public Process this[int index] {
            get { return choices[index]; }
        }

        public int Count {
            get { return choices.Count; }
        }
        
        public void Add(Process p) {
            choices.Add(p);
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
