using System.Collections.Generic;
using PLR.AST;
using PLR.Runtime;

namespace ProcessViewer {
    public class ProcessStateVisualization {

        List<ProcessBase> _procs;
        ProcessSystem _ast;
        
        public ProcessStateVisualization(List<ProcessBase> procs, ProcessSystem ast) {
            _ast = ast;
            _procs = procs;
            BuildState();
        }

        void BuildState() {
            foreach (ProcessBase p in _procs) {
                string name = p.ToString();
            }
        }
    }
}
