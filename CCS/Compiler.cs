using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using CCS.Parsing;
using PLR.AST;

namespace CCS {
    public class Compiler {

        public int Compile(string filename) {
            Parser parser = new Parser(new Scanner(new FileStream(filename, FileMode.Open)));
            parser.Parse();
            ProcessSystem system = parser.System;
            system.Compile("test.exe", "CCS");
            return 0;
        }
    }
}
