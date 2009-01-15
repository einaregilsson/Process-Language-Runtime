using System.Collections.Generic;

namespace CCS.Nodes {

    public class CCSSystem : ASTNode, IEnumerable<ProcessDefinition> {

        private List<ProcessDefinition> processes = new List<ProcessDefinition>();

        public IEnumerator<ProcessDefinition> GetEnumerator() {
            return processes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return processes.GetEnumerator();
        }

        public ProcessDefinition this[int index] {
            get { return processes[index]; }
        }

        public int Count {
            get { return this.processes.Count; }
        }

        public void Add(ProcessDefinition pd) {
            this.processes.Add(pd);
        }

        public override List<ASTNode> GetChildren() {
            var l = new List<ASTNode>();
            foreach (ASTNode a in processes) {
                l.Add(a);
            }
            return l;
        }

    }
}
