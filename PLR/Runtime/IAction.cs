using System;
using System.Collections.Generic;
using System.Text;

namespace PLR.Runtime {
    public interface IAction {
        bool CanSyncWith(IAction other);
        int ProcessID { get; }
        bool IsAsynchronous { get; }
    }
}