using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CCS.Parsing;
using PLR;
using PLR.AST;
using PLR.AST.Processes;
using CCS.Formatters;
using System.Reflection;
using System.Security.Cryptography;

namespace CCS {
    class Program {

        static int Main(string[] args) {
            if (args.Length == 0) {
                Console.Error.WriteLine(@"
CCS Compiler
Copyright (C) 2009 Einar Egilsson

Usage: CCS [options] <filename>
");
                return 1;
            }
            List<string> listArgs = new List<string>(args);
            CompileOptions options = CompileOptions.Parse(listArgs);

            DieIf(options.Arguments.Count == 0, "ERROR: Missing input file name");
            DieIf(options.Arguments.Count > 1, "ERROR: Only one input file is expected");
            string filename = listArgs[listArgs.Count - 1];
            DieIf(!File.Exists(filename), "ERROR: File '{0}' does not exist!", filename);
            if (options.OutputFile == null) {
                options.OutputFile = filename;
                if (options.OutputFile.ToLower().EndsWith(".ccs")) {
                    options.OutputFile = options.OutputFile.Substring(0, options.OutputFile.Length - 4);
                }
            }

            if (!options.OutputFile.ToLower().EndsWith(".exe")) {
                options.OutputFile += ".exe";
            }

            Parser parser = new Parser(new Scanner(new FileStream(filename, FileMode.Open)));
            parser.Parse();
            ProcessSystem system = parser.System;
            system.Compile(options);
            return 0;
        }

        private static void DieIf(bool condition, string msg, params object[] args) {
            if (condition) {
                Console.Error.WriteLine(string.Format(msg, args));
                System.Environment.Exit(1);
            }
        }

    }
}
