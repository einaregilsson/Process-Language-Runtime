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
using System.Text;
using System.IO;
using CCS.Parsing;
using PLR;
using PLR.Runtime;
using PLR.Compilation;
using PLR.AST;
using PLR.AST.Processes;
using System.Reflection;
using System.Security.Cryptography;
using PLR.AST.Formatters;
using PLR.Analysis;

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
            CompileOptions.AddOptionWithArgument("p", "print", ""); //Allow printouts
            CompileOptions options = CompileOptions.Parse(listArgs);
            
            DieIf(options.Arguments.Count == 0, "ERROR: Missing input file name");
            DieIf(options.Arguments.Count > 1, "ERROR: Only one input file is expected");
            string filename = options.Arguments[0];
            DieIf(!File.Exists(filename), "ERROR: File '{0}' does not exist!", filename);
            if (string.IsNullOrEmpty(options.OutputFile)) {
                options.OutputFile = filename;
                if (options.OutputFile.ToLower().EndsWith(".ccs")) {
                    options.OutputFile = options.OutputFile.Substring(0, options.OutputFile.Length - 4);
                }
            }

            if (!options.OutputFile.ToLower().EndsWith(".exe")) {
                options.OutputFile += ".exe";
            }
            try {

                Parser parser = new Parser(new Scanner(new FileStream(filename, FileMode.Open)));
                parser.Parse();
                if (parser.errors.count > 0) {
                    Environment.Exit(1);
                }
                ProcessSystem system = parser.System;
                system.MeetTheParents();
                List<Warning> warnings = system.Analyze(new UnusedAssignments(),new NilProcessRemoval());
                foreach (Warning warn in warnings) {
                    Console.Error.WriteLine("WARNING({0},{1}): {2}", warn.LexicalInfo.StartLine, warn.LexicalInfo.StartColumn, warn.Message);
                }

                CheckPrintout(options, system);
                system.Compile(options);
            } catch (Exception ex) {
                DieIf(true, "ERROR: " + ex.Message);
            }
            return 0;
        }

        private static void CheckPrintout(CompileOptions options, ProcessSystem system) {
            //Check whether we want to print out a formatted version of the source
            if (options["p"] != "") {
                string format = options["p"];
                BaseFormatter formatter = null;
                if (format.ToLower() == "html") {
                    formatter = new BaseFormatter();
                } else if (format.ToLower() == "latex" || format.ToLower() == "tex") {
                    format = "tex";
                    formatter = new LaTeXFormatter();
                } else if (format.ToLower() == "txt" || format.ToLower() == "ccs") {
                    format = "formatted_ccs"; //So we don't overwrite the original file...
                    formatter = new BaseFormatter();
                } else {
                    DieIf(true, "Format for /print options must be one of ccs,html,latex");
                }
                formatter.Start(system);
                string result = formatter.GetFormatted();
                string filename = Path.ChangeExtension(options.OutputFile, format);
                File.WriteAllText(filename, result);
                Console.WriteLine("Formatted source written to {0}", filename);
            }
        }

        private static void DieIf(bool condition, string msg, params object[] args) {
            if (condition) {
                Console.Error.WriteLine(string.Format(msg, args));
                System.Environment.Exit(1);
            }
        }

    }
}
