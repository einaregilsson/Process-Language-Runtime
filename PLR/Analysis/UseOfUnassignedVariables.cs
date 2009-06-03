/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System.Collections.Generic;
using PLR.AST;
using PLR.AST.Processes;
using PLR.AST.Expressions;

namespace PLR.Analysis {

    public class UseOfUnassignedVariables : AbstractVisitor, IAnalysis {

        private List<Warning> _warnings;

        public List<Warning> Analyze(ProcessSystem system) {
            base.VisitParentBeforeChildren = true; //Forward analysis
            _warnings = new List<Warning>();
            this.Start(system);
            return _warnings;
        }

        public override void Visit(ProcessDefinition def) {
            def.Tag = new Set(def.Variables);    
        }

        public override void Visit(Process p) {
            Set set = new Set();
            set.AddRange((Set)p.Parent.Tag);

            foreach (Variable var in p.ReadVariables) {
                if (!set.Contains(var)) {
                    _warnings.Add(new Warning(var.LexicalInfo, "Use of unassigned variable " + var.Name));
                }
            }
            set.AddRange(p.AssignedVariables);

            p.Tag = set;
        }
    }
}
