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

namespace PLR.Runtime {
    public static class GlobalScope {
        private static List<IAction> _actions = new List<IAction>();
        public static List<IAction> Actions {
            get { return _actions; }
        }
    }
}
