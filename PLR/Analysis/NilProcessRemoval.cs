/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST;
using PLR.AST.Expressions;
using PLR.AST.Actions;
using PLR.AST.Processes;

namespace PLR.Analysis {

    public class NilProcessRemoval : AbstractVisitor, IAnalysis {

        private List<Warning> _warnings;

        public List<Warning> Analyze(ProcessSystem system) {
            _warnings = new List<Warning>();
            this.Start(system);
            return _warnings;
        }

        public override void Visit(NilProcess np) {
            if (np.Parent is NonDeterministicChoice) {
                _warnings.Add(new Warning(np.LexicalInfo, "Nil process will never be chosen in non deterministic choice. P+0 == 0"));
            } else if (np.Parent is ParallelComposition) {
                _warnings.Add(new Warning(np.LexicalInfo, "A nil process has no effect in parallel composition. P | 0 == 0"));
            } else if (np.Parent is ProcessDefinition) {
                string procName = ((ProcessDefinition)np.Parent).Name;
                _warnings.Add(new Warning(np.Parent.LexicalInfo, "Process " + procName + " has no effect and can be replaced with the nil process as appropriate"));
            }
            
        }
    }
}
