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
            CompileOptions.AddOption("ek", "embedKLAIM");
            CompileOptions options = CompileOptions.Parse(listArgs);

            if (options.Help) {
                Console.Error.WriteLine(@"
KLAIM Compiler
Copyright (C) 2009 Einar Egilsson

Usage: kc [options] <filename>

Available options:

    /optimize                If specified then the generated assembly will be
    /op                      optimized, dead code eliminated and expressions
                             pre-evaluated where possible. Do not combine this
                             with the /debug switch.
    
    /debug                   Emit debugging symbols in the generated file,
    /d                       this allows it to be debugged in Visual Studio, or
                             in the free graphical debugger that comes with the
                             .NET Framework SDK.

    /embedKLAIM              Embeds the KLAIM runtime into the generated file, 
    /ek                      so it does not need to be distributed with the 
                             executable.

    /embedPLR                Embeds the PLR into the generated file, so it does
    /e                       not need to be distributed with the executable.

    /out:<filename>          Specify the name of the compiled executable. If 
    /o:<filename>            this is not specified then the name of the input
                             file is used, with .ccs replaced by .exe.
");

                return 1;
            }

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
            if (options.Contains("embedKLAIM")) {
                parser.Processes.EmbedAssembly(typeof(Locality).Assembly);
            }
            parser.Processes.MeetTheParents();
            
            List<Warning> warnings = parser.Processes.Analyze(new ReachingDefinitions());

            if (warnings.Count > 0) {
                foreach (Warning warn in warnings) {
                    Console.Error.WriteLine("ERROR({0},{1}): {2}", warn.LexicalInfo.StartLine, warn.LexicalInfo.StartColumn, warn.Message);
                }
                return 1; //This is an error so we die before attempting to compile
            }

            //These are just warnings, so just warn...
            warnings = parser.Processes.Analyze(new LiveVariables(), new NilProcessWarning());
            foreach (Warning warn in warnings) {
                Console.Error.WriteLine("WARNING({0},{1}): {2}", warn.LexicalInfo.StartLine, warn.LexicalInfo.StartColumn, warn.Message);
            }

            try {
                compiler.Compile(parser.LocatedTuples, parser.Processes, options);
            } catch (Exception ex) {
                DieIf(true, "ERROR: " +ex.Message);
            }
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

