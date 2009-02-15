using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using CCS.Parsing;
using PLR.AST;
using CCS.Formatters;

namespace CCS {
    class Program {

        static void Main(string[] args) {

            string text =
@"->User = User_{FOO}
";
            MemoryStream ms = new MemoryStream();
            StreamWriter w = new StreamWriter(ms);
            w.Write(text.Replace("FOO", args[0]));
            w.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            Parser p = new Parser(new Scanner(ms));
            p.Parse();
            //if (p.Errors.Count == 0) {
            //    Console.WriteLine(p.System);
            //    Analyzer analyzer = new Analyzer();
            //    List<CCSError> e = analyzer.Analyze(p.System);
            //    foreach (CCSError err in e) {
            //        Console.WriteLine(err);
            //    }
            //}
            //System.IO.File.WriteAllText("D:\\foo.html", "<html><body>" + new HtmlFormatter().Format(p.System) + "</body></html>");
            //System.IO.File.WriteAllText("D:\\foo.tex", new LaTeXFormatter().Format(p.System));
            //Console.Read();
            ProcessConstant pp = (ProcessConstant) p.System[0].Process;
            new CCS.Compiler.CCSCompiler().Compile(pp.Subscript[0]);

        }
    }
}
