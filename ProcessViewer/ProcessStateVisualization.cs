/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System.Collections.Generic;
using System.Text;
using PLR.AST;
using PLR.AST.Formatters;
using PLR.AST.Processes;
using PLR.Runtime;

namespace ProcessViewer {
    public class ProcessStateVisualization {

        ProcessSystem _ast;
        List<Node> _activeProcesses = new List<Node>();
        Dictionary<string, Process> _procConstants = new Dictionary<string, Process>();
        Dictionary<string, int> _performedActionsCount = new Dictionary<string, int>();
        string _string;
        BaseFormatter _formatter;

        public ProcessStateVisualization(ProcessSystem ast, BaseFormatter formatter) {
            _ast = ast;
            _formatter = formatter;
            foreach (ProcessDefinition pd in _ast.ChildNodes) {
                _procConstants.Add(pd.Name, pd.Process);
            }
        }

        public override string ToString() {
            return _string;
        }

        public void NewState(List<ProcessBase> activeProcs, CandidateAction chosenAction) {
            StringBuilder builder = new StringBuilder();

            if (chosenAction != null) {
                string p1 = chosenAction.Process1.ToString();
                if (!_performedActionsCount.ContainsKey(p1)) {
                    _performedActionsCount.Add(p1, 1);
                } else {
                    _performedActionsCount[p1]++;
                }

                if (chosenAction.Process2 != null) {
                    string p2 = chosenAction.Process2.ToString();
                    if (!_performedActionsCount.ContainsKey(p2)) {
                        _performedActionsCount.Add(p2, 1);
                    } else {
                        _performedActionsCount[p2]++;
                    }
                }
            }
            foreach (ProcessBase pb in activeProcs) {
                string[] nameparts = pb.ToString().Split('@');
                string name = nameparts[0];
                string id = nameparts[1];

                Node newProc = null;
                if (!name.Contains("+")) {
                    newProc = _procConstants[name];
                    newProc.Tag = id;
                    RemovePerformedActions(pb.ToString(), ref newProc);
                    _formatter.Start(newProc);
                    builder.Append("\n\n# " + pb.ToString() + ": \n" + FormatProc(_formatter.GetFormatted()));
                } else {
                    nameparts = name.Split('+');
                    newProc = _procConstants[nameparts[0]];
                    for (int i = 1; i < nameparts.Length; i++) {
                        string nextpart = nameparts[i];
                        if (nextpart.StartsWith("NonDeterministic")) {
                            while (!(newProc is NonDeterministicChoice)) {
                                if (newProc is ActionPrefix) {
                                    newProc = ((ActionPrefix)newProc).Process;
                                }
                            }
                            int index = int.Parse(nextpart.Substring("NonDeterministic".Length));
                            newProc = ((NonDeterministicChoice)newProc).Processes[index - 1];
                        }
                        else if (nextpart.StartsWith("Parallel")) {
                            while (!(newProc is ParallelComposition)) {
                                if (newProc is ActionPrefix) {
                                    newProc = ((ActionPrefix)newProc).Process;
                                }
                            }
                            int index = int.Parse(nextpart.Substring("Parallel".Length));
                            newProc = ((ParallelComposition)newProc).Processes[index - 1];
                        } else if (nextpart.StartsWith("Inner")) {
                            while (newProc.ChildNodes[0] == null && newProc.ChildNodes[1] == null) {
                                newProc = ((ActionPrefix)newProc).Process;
                            }
                        }

                    }
                    RemovePerformedActions(pb.ToString(), ref newProc);
                    _formatter.Start(newProc);
                    builder.Append("\n\n# " + pb.ToString() + ": \n" + FormatProc(_formatter.GetFormatted()));

                }
            }
            _string = builder.ToString();
        }

        private void RemovePerformedActions(string procName, ref Node proc) {
            if (!_performedActionsCount.ContainsKey(procName)) {
                return;
            }
            int count = _performedActionsCount[procName];
            for (int i = 0; i < count; i++) {
                proc = ((ActionPrefix)proc).Process;
            }
        }

        private string FormatProc(string proc) {
            if (proc.Contains("+") && proc.IndexOf("+") > 20) {
                proc = "  " + proc.Replace("+", "\n+ ");
            }
            return proc;
        }
    }
}
