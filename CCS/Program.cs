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

            if (args.Length == 0) {
                Console.WriteLine("CCS Interpreter\nUsage: CCS [-interactive] <filename>");
                return;
            }
            Parser p = new Parser(new Scanner(new FileStream(args[args.Length-1], FileMode.Open)));
            p.Parse();
            if (p.errors.count > 0) {
                return;
            }
            Interpreter i = new Interpreter(p.System, args.Length == 2 && args[0].ToLower() == "-interactive");
            long counter = 0;
            //while (true) {
            //    counter++;
            //    i.Iterate(System.Console.Out, delegate() { return int.Parse(Console.ReadLine()); });
            //    if (counter % 50 == 0) {
            //        Console.WriteLine("{0} iterations finished, press Ctrl-C to exit or any other key to continue", counter);
            //        Console.ReadKey();
            //    }
            //}
           
        }
    }
}
