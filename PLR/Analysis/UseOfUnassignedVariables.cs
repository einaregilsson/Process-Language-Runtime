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
    public class UseOfUnassignedVariables : AbstractVisitor, IAnalysis {
        private ProcessDefinition _currentProc;
        private List<string> _declaredVariables = new List<string>();
        private List<Warning> _warnings;
        
        public List<Warning> Analyze(ProcessSystem system) {
            _warnings = new List<Warning>();
            this.Start(system);
            return _warnings;
        }

        public override void Visit(ProcessDefinition node) {
            _currentProc = node;
            _declaredVariables.Clear();
            foreach (Variable var in node.Variables) {
                _declaredVariables.Add(var.Name);
            }
        }

        public override void Visit(NonDeterministicChoice ndc) {
        }

        public override void Visit(ParallelComposition pc) {
        }

        public override void Visit(BranchProcess bp) {
        }

        public override void Visit(InAction act) {
            foreach (Variable var in act) {
                _declaredVariables.Add(var.Name);
            }
        }

        public override void Visit(Variable var) {
            if (!(var.Parent is InAction || (var.Parent is ExpressionList && var.Parent.Parent is ProcessDefinition))) {
                if (!_declaredVariables.Contains(var.Name)) {
                    _warnings.Add(new Warning(var.LexicalInfo, "Use of unassigned variable " + var.Name));
                }
            }
        }
    }
}
