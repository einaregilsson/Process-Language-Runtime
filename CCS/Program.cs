using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CCS.Parsing;
using PLR.AST;
using PLR.AST.Processes;
using CCS.Formatters;

namespace CCS {
    class Program {

        static int Main(string[] args) {
            EINAR.NewProc s = new EINAR.NewProc();
            s.Start();
            Parser parser = new Parser(new Scanner(new FileStream(args[0], FileMode.Open)));
            parser.Parse();
            parser.System.Compile("test.dll", "CCS");
            return 0;
        }
    }
}
