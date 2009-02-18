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

        static void Main(string[] args) {

            string text =
@"->User = a . b . User
->John = _a_ . _b_ . _a_ .0
->Siggi = _a_.b.0+_a_.0
";
            MemoryStream ms = new MemoryStream();
            StreamWriter w = new StreamWriter(ms);
            w.Write(text);
            w.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            Parser p = new Parser(new Scanner(ms));
            p.Parse();
            Interpreter i = new Interpreter();
            i.Interpret(p.System);
        }
    }
}
