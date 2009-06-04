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
using PLR.Analysis.Actions;
using ActionCollection = System.Collections.IEnumerable;
using PLR.Analysis.ActionHandling;

namespace PLR.Analysis {

    public class UnmatchedChannels : AbstractVisitor, IAnalysis {
        private List<KeyValuePair<string,string>> _mappings = new List<KeyValuePair<string,string>>();
        private List<Warning> _warnings;
        private List<InAction> _inActions = new List<InAction>();
        private List<OutAction> _outActions = new List<OutAction>();

        public List<Warning> Analyze(ProcessSystem system) {
            base.VisitParentBeforeChildren = false;
            _warnings = new List<Warning>();
            this.Start(system);
            Match(_inActions, _outActions, "output");
            Match(_outActions, _inActions, "input");
            return _warnings;
        }

        public override void Visit(InAction act) {
            _inActions.Add(act);
        }

        public override void Visit(OutAction act) {
            _outActions.Add(act);
        }

        public override void Visit(RelabelActions relabel) {
            foreach (KeyValuePair<string,string> kv in relabel.Mapping) {
                if (!_mappings.Contains(kv)) {
                    _mappings.Add(kv);
                }
            }
        }


        public void Match(ActionCollection check, ActionCollection other, string type) {
            foreach (Action checkAct in check) {
                bool hasMatch = false;
                bool hasMatchWithWrongParamCount = false;
                foreach (Action otherAct in other) {
                    if (checkAct.Name == otherAct.Name) {

                        if (checkAct.ChildNodes.Count == otherAct.ChildNodes.Count) {
                            hasMatch = true;
                            break;
                        } else {
                            hasMatchWithWrongParamCount = true;
                        }
                    }
                }
                if (!hasMatch && hasMatchWithWrongParamCount) {
                    _warnings.Add(new Warning(checkAct.LexicalInfo, "This action will block forever, as there is no possible " + type + " on channel " + checkAct.Name + " which has " + checkAct.ChildNodes.Count + " parameters"));
                    ((ActionPrefix)checkAct.Parent).Process.IsUsed = false;
                } else if (!hasMatch) {
                    _warnings.Add(new Warning(checkAct.LexicalInfo, "This action will block forever, as there is no " + type + " on channel " + checkAct.Name));
                    ((ActionPrefix)checkAct.Parent).Process.IsUsed = false;
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
                if (!set.Contains(var) && var.Name != "dummy") {
                    _warnings.Add(new Warning(var.LexicalInfo, "This assignment to variable " + var.Name + " has no effect, it is never read"));
                }
                set.Remove(var);
            }
            p.Tag = set;
        }
    }
}
