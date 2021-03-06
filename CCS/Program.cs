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
using PLR.Analysis;
using PLR.AST.Processes;
using System.Reflection;
using System.Security.Cryptography;
using PLR.AST.Formatters;
using PLR.AST;

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

            if (options.Help) {
                Console.Error.WriteLine(@"
CCS Compiler
Copyright (C) 2009 Einar Egilsson

Usage: CCS [options] <filename>

Available options:

    /reference:<filenames>   The assemblies that this program requires. It is 
    /r:<filenames>           not neccessary to specify the PLR assembly. 
                             Other assemblies should be specified in a comma
                             seperated list, e.g. /reference:Foo.dll,Bar.dll.
    
    /optimize                If specified then the generated assembly will be
    /op                      optimized, dead code eliminated and expressions
                             pre-evaluated where possible. Do not combine this
                             with the /debug switch.
    
    /embedPLR                Embeds the PLR into the generated file, so it can
    /e                       be distributed as a stand-alone file.
  
    /debug                   Emit debugging symbols in the generated file,
    /d                       this allows it to be debugged in Visual Studio, or
                             in the free graphical debugger that comes with the
                             .NET Framework SDK.

    /out:<filename>          Specify the name of the compiled executable. If 
    /o:<filename>            this is not specified then the name of the input
                             file is used, with .ccs replaced by .exe.

    /print:<format>          Prints a version of the program source in the
    /p:<format>              specified format. Allowed formats are ccs, html 
                             and latex. The generated file will have the same
                             name as the input file, except with the format
                             as extension.

");

                return 1;
            }

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
                List<Warning> warnings = system.Analyze(new ReachingDefinitions());

                if (warnings.Count > 0) {
                    foreach (Warning warn in warnings) {
                        Console.Error.WriteLine("ERROR({0},{1}): {2}", warn.LexicalInfo.StartLine, warn.LexicalInfo.StartColumn, warn.Message);
                    }
                    return 1; //This is an error so we die before attempting to compile
                }

                //These are just warnings, so just warn...
                warnings = system.Analyze(
                    new LiveVariables(), 
                    new NilProcessWarning(), 
                    new UnmatchedChannels(), 
                    new UnusedProcesses(options.Optimize)
                );

                foreach (Warning warn in warnings) {
                    Console.Error.WriteLine("WARNING({0},{1}): {2}", warn.LexicalInfo.StartLine, warn.LexicalInfo.StartColumn, warn.Message);
                }

                //This optimizes the tree before compilation, only do if we should optimize
                if (options.Optimize) {
                    system.Analyze(new FoldConstantExpressions());
                }

                CheckPrintout(options, system);
                system.Compile(options);
            } catch (Exception ex) {
                //Console.WriteLine(ex.StackTrace);
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
