using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST;
using PLR.AST.Processes;
using PLR.AST.Actions;
using PLR.AST.Expressions;

namespace PLR {
    public class Program {
        public static void Main() {
            ProcA pa = new ProcA();
            ProcB pb = new ProcB();
            //pa.Run();
            //pb.Run();
            //ProcessBase._scheduler.Run();
            ProcessSystem system = new ProcessSystem();
            ProcessDefinition def = new ProcessDefinition(new ProcessConstant("NewProc"), new NilProcess(), true);
            system.Add(def);
            system.Compile("out.dll", "NewNamespace");
        }
    }
}
