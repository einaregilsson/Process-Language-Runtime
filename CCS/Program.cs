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
            string file = args[args.Length-1];
            FileStream stream = new FileStream(file, FileMode.Open);
            Parser p = new Parser(new Scanner(stream));
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            p.Parse();
            Console.ForegroundColor = original;
            stream.Close();
            if (p.errors.count > 0) {
                return 2;
            }
            Console.WriteLine(File.ReadAllText(file));
            Interpreter i = new Interpreter(p.System, args.Length == 2 && args[0].ToLower() == "-interactive");
            return i.RunConsole();
        }
    }
}
