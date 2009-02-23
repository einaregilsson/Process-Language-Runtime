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

            if (args.Length == 0) {
                Console.Error.WriteLine("CCS Interpreter\nUsage: CCS [-interactive] <filename>");
                return 1;
            }
            Parser p = new Parser(new Scanner(new FileStream(args[args.Length-1], FileMode.Open)));
            p.Parse();
            if (p.errors.count > 0) {
                return 2;
            }
            Interpreter i = new Interpreter(p.System, args.Length == 2 && args[0].ToLower() == "-interactive");
            return i.RunConsole();
        }
    }
}
