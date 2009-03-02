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
            while (true) {
                if (pa.Waiting && pb.Waiting) {
                    ProcessBase._scheduler.FindMatches();
                }
                System.Threading.Thread.Sleep(100);
            }
            Console.ReadKey();
        }
    }
}
