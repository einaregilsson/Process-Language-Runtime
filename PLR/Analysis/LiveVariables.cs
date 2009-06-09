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

    public class LiveVariables : AbstractVisitor, IAnalysis {

        private List<Warning> _warnings;

        public List<Warning> Analyze(ProcessSystem system) {
            base.VisitParentBeforeChildren = false; //Backwards analysis
            _warnings = new List<Warning>();
            this.Start(system);
            return _warnings;
        }

        public override void Visit(ProcessDefinition def) {
            foreach (Variable var in def.Variables) {
                if (!((Set)def.Process.Tag).Contains(var) && var.Name != Variable.NotUsedName) {
                    _warnings.Add(new Warning(var.LexicalInfo, "The initial value of " + var.Name + " which is passed to " + def.Name + " is never read in the process"));
                    var.IsUsed = false;
                }
            }
        }

        public override void Visit(Process p) {
            Set set = new Set();
            foreach (Process child in p.FlowsTo) {
                set.AddRange((Set)child.Tag);
            }

            set.AddRange(p.ReadVariables);

            foreach (Variable var in p.AssignedVariables) {
                if (!set.Contains(var) && var.Name != Variable.NotUsedName) {
                    _warnings.Add(new Warning(var.LexicalInfo, "This assignment to variable " + var.Name + " has no effect, it is never read"));
                    var.IsUsed = false;
                }
                set.Remove(var);
            }
            p.Tag = set;
        }
    }
}
