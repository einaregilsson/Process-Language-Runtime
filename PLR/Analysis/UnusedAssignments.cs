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
using PLR.AST.Interfaces;

namespace PLR.Analysis {

    public class UnusedAssignments : AbstractVisitor, IAnalysis {
    
        private List<Variable> _readVariables = new List<Variable>();
        private List<Warning> _warnings;

        public List<Warning> Analyze(ProcessSystem system) {
            _warnings = new List<Warning>();
            this.Start(system);
            return _warnings;
        }

        public override void Visit(ProcessDefinition node) {
            foreach (Variable var in node.Variables) {
                if (!_readVariables.Contains(var)) {
                    _warnings.Add(new Warning(var.LexicalInfo, "Variable " + var.Name + " is never read in the process " + node.Name));
                };
            }
            //Now clear so everything is ready for the next proc def
            _readVariables.Clear();
        }

        public override void Visit(IVariableAssignment ass) {
            if (!(ass is ProcessDefinition)) {
                foreach (Variable var in ass.AssignedVariables) {
                    if (!_readVariables.Contains(var)) {
                        _warnings.Add(new Warning(var.LexicalInfo, "Variable " + var.Name + " is never read after assignment"));
                    }
                }
            }
        }

        public override void Visit(IVariableReader reader) {
            foreach (Variable v in reader.ReadVariables) {
                if (!_readVariables.Contains(v)) {
                    _readVariables.Add(v);
                }
            }
        }
    }
}
