using System;
using System.Collections.Generic;
using System.Text;

namespace PLR.Runtime {
    public interface IAction {
        bool CanSyncWith(IAction other);
        void Sync(IAction other);
        int ProcessID { get; }
        bool IsAsynchronous { get; }
    }
}
