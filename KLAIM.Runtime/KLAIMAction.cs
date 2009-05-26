/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System;
using System.Collections.Generic;
using System.Text;
using PLR.Runtime;

namespace KLAIM.Runtime {
    public class KLAIMAction : IAction{

        private ProcessBase _proc;
        private string _displayName;
        public KLAIMAction(ProcessBase p, string displayName) {
            _proc = p;
            _displayName = displayName;
        }

        public bool CanSyncWith(IAction other) {
            return false;
        }

        public override string ToString() {
            return _displayName;
        }

        public void Sync(IAction other) {
            //Do nothing
        }

        public int ProcessID {
            get { return _proc.ID; }
        }

        public bool IsAsynchronous {
            get { return true; }
        }

    }
}
