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

        private void UpdateProcessActions(ProcessBase p) {
            string name = p.ToString();
            if (!_performedActionsCount.ContainsKey(name)) {
                _performedActionsCount.Add(name, 1);
            } else {
                Node proc = GetProcess(name);
                int count = 0;
                while (proc is ActionPrefix) {
                    proc = ((ActionPrefix)proc).Process;
                    count++;
                }
                int countPerformed = _performedActionsCount[name];
                //System.Windows.Forms.MessageBox.Show("Count: " + count + ", countperformed: " + countPerformed);
                if (countPerformed == count-1) {
                    _performedActionsCount[name] = 0;
                } else {
                    _performedActionsCount[name]++;
                }
            }
        }

        public void NewState(List<ProcessBase> activeProcs, CandidateAction chosenAction) {
            StringBuilder builder = new StringBuilder();

            if (chosenAction != null) {
                UpdateProcessActions(chosenAction.Process1);
                if (chosenAction.Process2 != null) {
                  UpdateProcessActions(chosenAction.Process2);
                }
            }
            foreach (ProcessBase pb in activeProcs) {
                string name = pb.ToString();
                Node proc = GetProcess(name);
                RemovePerformedActions(name, ref proc);
                builder.Append("\n\n# " + name + ": \n");
                if (pb.Parent != null) {
                    builder.Append("# Parent chain: ");
                    ProcessBase parent = pb.Parent;

                    List<string> parts = new List<string>();
                    while (parent != null) {
                        Node parentProc = GetProcess(parent.ToString());
                        string oneParent = parent + ": ";
                        if (parentProc.ChildNodes[0] != null) {
                            _formatter.Start(parentProc.ChildNodes[0]);
                            oneParent += (_formatter.GetFormatted());
                        }
                        if (parentProc.ChildNodes[1] != null) {
                            _formatter.Start(parentProc.ChildNodes[1]);
                            oneParent += _formatter.GetFormatted();
                        }
                        parts.Add(oneParent);
                        parent = parent.Parent;
                    }
                    parts.Reverse();
                    builder.Append(string.Join("; ", parts.ToArray()));
                    builder.Append("\n");
                }
                _formatter.Start(proc);
                builder.Append(FormatProc(_formatter.GetFormatted()));
            }
            _string = builder.ToString();
        }

        private Node GetProcess(string procName) {
            string name = procName.Substring(0, procName.IndexOf('@'));
            string id = procName.Substring(procName.IndexOf('@') + 1);

            if (!name.Contains("+")) {
                return _procConstants[name];
            }

            Node proc;

            string[] nameparts = name.Split('+');
            proc = _procConstants[nameparts[0]];

            for (int i = 1; i < nameparts.Length; i++) {
                string nextpart = nameparts[i];
                if (nextpart.StartsWith("NonDeterministic")) {
                    while (!(proc is NonDeterministicChoice)) {
                        proc = ((ActionPrefix)proc).Process;
                    }
                    int index = int.Parse(nextpart.Substring("NonDeterministic".Length));
                    proc = ((NonDeterministicChoice)proc).Processes[index - 1];
                } else if (nextpart.StartsWith("Parallel")) {
                    while (!(proc is ParallelComposition)) {
                        proc = ((ActionPrefix)proc).Process;
                    }
                    int index = int.Parse(nextpart.Substring("Parallel".Length));
                    proc = ((ParallelComposition)proc).Processes[index - 1];
                } else if (nextpart.StartsWith("Inner")) {
                    while (proc.ChildNodes[0] == null && proc.ChildNodes[1] == null) {
                        proc = ((ActionPrefix)proc).Process;
                    }
                    proc = (Process)proc;
                }
            }
            return (Process)proc;
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
