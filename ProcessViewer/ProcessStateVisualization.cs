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

            foreach (ProcessBase pb in activeProcs) {
                string[] nameparts = pb.ToString().Split('@');
                string name = nameparts[0];
                string id = nameparts[1];
                
                Node newProc = null;
                if (!name.Contains("+")) {
                    newProc = _procConstants[name];
                    newProc.Tag = id;
                    _activeProcesses.Add(newProc);
                    _formatter.Start(newProc);
                    builder.Append("\n\n\n" + FormatProc(_formatter.GetFormatted())); 
                } else {
                    nameparts = name.Split('+');
                    newProc = _procConstants[name];
                    for (int i = 1; i < nameparts.Length; i++) {
                        string nextpart = nameparts[i];
                    }
                }
                _string = builder.ToString();
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
