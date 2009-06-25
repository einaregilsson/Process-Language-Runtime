/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */

namespace PLR.Runtime {
    
    public class CandidateAction {

        private static readonly CandidateAction _deadlock = new CandidateAction(null, null, null, null, null);
        public static CandidateAction DEADLOCK { get { return _deadlock; } }

        #region Properties
        public IAction Action1 { get; set; }
        public IAction Action2 { get; set; }
        public ProcessBase Process1 { get; set; }
        public ProcessBase Process2 { get; set; }
        public ProcessBase ScopeProcess { get; set; }

        public bool IsDeadlocked {
            get {
                return this == CandidateAction.DEADLOCK;
            }
        }

        public bool IsTau {
            get {
                return ScopeProcess != null;
            }
        }

        public bool IsAsync {
            get {
                return Action1 != null && Action1.IsAsynchronous;
            }
        }

        public bool IsSync {
            get {
                return Action1 != null && Action2 != null;
            }
        }

        #endregion


        public CandidateAction(IAction a1, IAction a2, ProcessBase p1, ProcessBase p2, ProcessBase scopeProcess) {
            Action1 = a1;
            Action2 = a2;
            Process1 = p1;
            Process2 = p2;
            ScopeProcess = scopeProcess;
        }

        public override string ToString() {
            if (IsDeadlocked) {
                return "<DEADLOCKED>";
            } else if (IsTau) {
                return "t - " + Action1.ToString().Replace("_","");
            } else {
                return Action1.ToString().Replace("_","");
            }
        }
    }
}
