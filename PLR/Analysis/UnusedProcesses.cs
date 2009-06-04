/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System.Collections.Generic;
using PLR.Analysis;
using PLR.Analysis.Processes;
using PLR.Analysis.Expressions;

namespace PLR.Analysis {

    public class UnusedProcesses : AbstractVisitor, IAnalysis {

        public UnusedProcesses(bool removeUnused) {
            _removeUnused = removeUnused;
        }

        private bool _removeUnused;
        private List<Warning> _warnings;
        private List<string> _procNames = new List<string>();
        public List<Warning> Analyze(ProcessSystem system) {
            _warnings = new List<Warning>();
            foreach (ProcessDefinition def in system) {
                _procNames.Add(def.Name);
            }
            this.Start(system);
            foreach (string name in _procNames) {
                for (int i = system.ChildNodes.Count-1; i >= 0; i--) {
                    ProcessDefinition def = (ProcessDefinition)system.ChildNodes[i];
                    if (def.Name == name && !def.EntryProc) {
                        _warnings.Add(new Warning(def.LexicalInfo, "Process " + def.Name + " is never invoked in the system"));
                        if (_removeUnused) {
                            system.ChildNodes.Remove(def);
                        }
                    }
                }
            }
            return _warnings;
        }


        public override void Visit(ProcessConstant p) {
            if (_procNames.Contains(p.Name)) {
                _procNames.Remove(p.Name);
            }
        }
    }
}
