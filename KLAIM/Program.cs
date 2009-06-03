/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLAIM.Parsing;
using KLAIM.Runtime;
using PLR.Compilation;
using System.Reflection.Emit;
using System.IO;
using PLR.Analysis;

namespace KLAIM {
    class Program {
        static int Main(string[] args) {
            if (args.Length == 0) {
                Console.Error.WriteLine(@"
KLAIM Compiler
Copyright (C) 2009 Einar Egilsson

Usage: kc [options] <filename>
");
                return 1;
            }
            List<string> listArgs = new List<string>(args);
            CompileOptions options = CompileOptions.Parse(listArgs);

            DieIf(options.Arguments.Count == 0, "ERROR: Missing input file name");
            DieIf(options.Arguments.Count > 1, "ERROR: Only one input file is expected");
            string filename = options.Arguments[0];
            DieIf(!File.Exists(filename), "ERROR: File '{0}' does not exist!", filename);
            if (string.IsNullOrEmpty(options.OutputFile)) {
                options.OutputFile = filename;
                if (options.OutputFile.ToLower().EndsWith(".klaim")) {
                    options.OutputFile = options.OutputFile.Substring(0, options.OutputFile.Length - 6);
                }
            }

            if (!options.OutputFile.ToLower().EndsWith(".exe")) {
                options.OutputFile += ".exe";
            }

            Parser parser = new Parser(new Scanner(new FileStream(filename, FileMode.Open)));
            parser.Parse();
            Compiler compiler = new Compiler();
            parser.Processes.MeetTheParents();
            List<Warning> warnings = parser.Processes.Analyze(new UnusedAssignments(), new UseOfUnassignedVariables());
            foreach (Warning warn in warnings) {
                Console.Error.WriteLine("WARNING({0},{1}): {2}", warn.LexicalInfo.StartLine, warn.LexicalInfo.StartColumn, warn.Message);
            }

            compiler.Compile(parser.LocatedTuples, parser.Processes, options);
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

