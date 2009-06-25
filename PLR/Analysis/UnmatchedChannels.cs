using System;
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
using PLR.AST.ActionHandling;
using PLR.AST.Actions;
using PLR.AST.Expressions;
using PLR.AST.Processes;

namespace PLR.Analysis {

    public class UnmatchedChannels : AbstractVisitor, IAnalysis {
        private List<KeyValuePair<string, string>> _mappings = new List<KeyValuePair<string, string>>();
        private List<Warning> _warnings;
        private List<Action> _inActions = new List<Action>();
        private List<Action> _outActions = new List<Action>();

        public List<Warning> Analyze(ProcessSystem system) {
            DateTime start = DateTime.Now;
            base.VisitParentBeforeChildren = false;
            _warnings = new List<Warning>();
            this.Start(system);
            GeneratePossibleChannels(_inActions);
            GeneratePossibleChannels(_outActions);
            Match(_inActions, _outActions, "output");
            Match(_outActions, _inActions, "input");
            TimeSpan t = DateTime.Now.Subtract(start);
            //TODO: Measure on larger systems...
            //Console.WriteLine("Unmatched channels finished in {0}ms", t.TotalMilliseconds); 
            return _warnings;
        }

        private void GeneratePossibleChannels(List<Action> actions) {
            foreach (Action act in actions) {
                var aliases = new List<string>();
                aliases.Add(act.Name);
                bool changed = false;
                do {
                    changed = false;
                    foreach (KeyValuePair<string, string> map in _mappings) {
                        if (!aliases.Contains(map.Value) && aliases.Contains(map.Key)) {
                            aliases.Add(map.Value);
                            changed = true;
                        }
                    }
                } while (changed);
                act.Tag = aliases;
            }
        }

        public override void Visit(InAction act) {
            _inActions.Add(act);
        }

        public override void Visit(OutAction act) {
            _outActions.Add(act);
        }

        public override void Visit(RelabelActions relabel) {
            foreach (KeyValuePair<string, string> kv in relabel.Mapping) {
                if (!_mappings.Contains(kv)) {
                    _mappings.Add(kv);
                }
            }
        }

        public void Match(List<Action> check, List<Action> other, string type) {
            foreach (Action checkAct in check) {
                bool hasMatch = false;
                bool hasMatchWithWrongParamCount = false;
                foreach (Action otherAct in other) {
                    if (NameMatch(checkAct, otherAct)) {

                        if (checkAct.ChildNodes.Count == otherAct.ChildNodes.Count) {
                            hasMatch = true;
                            break;
                        } else {
                            hasMatchWithWrongParamCount = true;
                        }
                    }
                }
                if (!hasMatch && hasMatchWithWrongParamCount) {
                    _warnings.Add(new Warning(checkAct.LexicalInfo, "This action will block forever, as there is no possible " + type + " on channel " + checkAct.Name + " (including if it is relabelled) which has " + checkAct.ChildNodes.Count + " parameters"));
                    ((ActionPrefix)checkAct.Parent).Process.IsUsed = false;
                } else if (!hasMatch) {
                    _warnings.Add(new Warning(checkAct.LexicalInfo, "This action will block forever, as there is no " + type + " on channel " + checkAct.Name + " (including if it is relabelled)"));
                    ((ActionPrefix)checkAct.Parent).Process.IsUsed = false;
                }
            }
        }

        private bool NameMatch(Action a, Action b) {
            List<string> aAliases = (List<string>)a.Tag;
            List<string> bAliases = (List<string>)b.Tag;

            return aAliases.Find(delegate(string s) { return bAliases.Contains(s); }) != null;
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
