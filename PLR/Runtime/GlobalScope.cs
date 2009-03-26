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
