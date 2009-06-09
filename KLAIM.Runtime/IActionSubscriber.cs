using System;
using System.Collections.Generic;
using System.Text;

namespace KLAIM.Runtime {
    public interface IActionSubscriber {
        void NotifyAction(int actionID);
    }
}
