using System;
using System.Collections.Generic;
using System.Text;

namespace PLR {
    public class Program {
        public static void Main() {
            ProcA pa = new ProcA();
            ProcB pb = new ProcB();
            pa.Run();
            pb.Run();
            ProcessBase._scheduler.Run();
        }
    }
}
