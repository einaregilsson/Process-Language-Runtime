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
using PLR.AST.Interfaces;

namespace PLR.Analysis {
    
    public class UseOfUnassignedVariables : AbstractVisitor, IAnalysis {
    
        private List<string> _declaredVariables = new List<string>();
        private List<Warning> _warnings;
        
        public List<Warning> Analyze(ProcessSystem system) {
            _warnings = new List<Warning>();
            this.Start(system);
            return _warnings;
        }

        public override void Visit(IVariableReader reader) {
            Node parent = ((Node)reader).Parent;
            List<Variable> readVars = reader.ReadVariables;
            if (readVars.Count == 0) {
                return;
            }

            while (parent != null) {
                if (parent is IVariableAssignment) {
                    foreach (Variable v in ((IVariableAssignment)parent).AssignedVariables) {
                        if (readVars.Contains(v)) {
                            readVars.Remove(v);
                        }
                    }
                }
                parent = parent.Parent;
            }
            foreach (Variable v in readVars) {
                _warnings.Add(new Warning(v.LexicalInfo, "Variable " + v.Name + " is used without being assigned first"));
            }
        }
    }
}
