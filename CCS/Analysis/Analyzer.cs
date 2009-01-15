using System;
using System.Collections.Generic;
using CCS.Nodes;

namespace CCS.Analysis {
    public class Analyzer {

        private List<CCSError> errors = new List<CCSError>();
        private CCSSystem system;
        
        public List<CCSError> Analyze(CCSSystem system) {
            this.system = system;
            AnalyzeSubscriptVariables();
            AnalyzeProcessConstants();
            return this.errors;
        }

        private void AnalyzeSubscriptVariables() {
            List<string> scope = new List<string>();
            foreach (ProcessDefinition def in system) {
                scope.Clear();
                foreach (ArithmeticExpression aexp in def.ProcessConstant.Subscript) {
                    if (aexp is Variable) {
                        Variable v = (Variable) aexp;
                        if (scope.Contains(v.Name)) {
                            errors.Add(new CCSError("Variable " + v.Name + " is already defined in this process", v.Line, v.Col));
                        }
                        scope.Add(((Variable)aexp).Name);
                    }
                }

                List<ASTNode> variableUses = new List<ASTNode>();
                GetAllNodesOfType(typeof(Variable), def.Process, variableUses);
                foreach (Variable v in variableUses) {
                    if (!scope.Contains(v.Name)) {
                        this.errors.Add(new CCSError("Variable " + v.Name + " is not defined!", v.Line, v.Col));
                    }
                }
                foreach (string var in scope) {
                    bool used = false;
                    foreach (Variable v in variableUses) {
                        if (v.Name == var) {
                            used = true;
                            break;
                        }
                    }
                    if (!used) {
                        errors.Add(new CCSError("Variable " + var + " is never used", def.ProcessConstant.Subscript.Line, def.ProcessConstant.Subscript.Col));
                    }
                }
            }
        }

        private void AnalyzeProcessConstants() {
            List<ASTNode> defined= new List<ASTNode>();
            List<ASTNode> invoked = new List<ASTNode>();
            bool hasEntry = false;
            foreach (ProcessDefinition def in system) {
                defined.Add(def.ProcessConstant);
                hasEntry |= def.EntryProc;
                GetAllNodesOfType(typeof(ProcessConstant), def.Process, invoked);

                if (def.EntryProc) {
                    foreach (ArithmeticExpression aexp in def.ProcessConstant.Subscript) {
                        if (aexp is Variable) {
                            errors.Add(new CCSError("Entry process '" + def.ProcessConstant.Name + "' cannot have subscript variables!", def.Line, def.Col));
                            break;
                        }
                    }
                }
            }
            if (!hasEntry) {
                errors.Add(new CCSError("System has no entry process!", 1, 1));
            }

            foreach (ProcessConstant inv in invoked) {
                bool foundName = false;
                bool foundNumber = false;
                foreach (ProcessConstant def in defined) {
                    if (inv.Name == def.Name) {
                        foundName = true;   
                    }
                    if (inv.Name == def.Name && inv.Subscript.Count == def.Subscript.Count) {
                        foundName = true;
                        foundNumber = true;
                    }
                }
                if (!foundName) {
                    errors.Add(new CCSError("No defined process with name '" + inv.Name + "'", inv.Line, inv.Col));
                } else if (!foundNumber) {
                    errors.Add(new CCSError("No process '" + inv.Name + "' takes " + inv.Subscript.Count + " arguments", inv.Line, inv.Col));
                }
            }
        }

        private void GetAllNodesOfType(Type type, ASTNode root, List<ASTNode> list) {
            foreach (ASTNode anode in root.GetChildren()) {
                if (anode.GetType().Equals(type)) {
                    list.Add(anode);
                }
                GetAllNodesOfType(type, anode, list);
            }
        }
    }
}
