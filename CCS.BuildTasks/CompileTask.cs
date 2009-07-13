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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace CCS.BuildTasks {
    public class CompileTask : ToolTask {

        [Required]
        public bool Debug { get; set; }

        [Required]
        public string OutputFile { get; set; }

        [Required]
        public string InputFile { get; set; }

        [Required]
        public string References { get; set; }

        protected override string GenerateCommandLineCommands() {
            string args = "";
            if (Debug) {
                args += " /debug ";
            }
            args += " /out:\"" + OutputFile + "\" ";
            if (!String.IsNullOrEmpty(References)) {
                args += " /reference:\"" + References.Replace(";",",") + "\" ";
            }
            args += InputFile;
            return args;        
        }

        public override bool Execute() {
            return base.Execute();
        }

        protected override void  LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            Match m = Regex.Match(singleLine, @"(WARNING|ERROR)(\(\d+,\d+\))?:(.*)");
            if (m.Success) {
                string type = m.Groups[1].Value;
                string numbers = m.Groups[2].Value;
                string message = m.Groups[3].Value;
                int line=0, column=0;
                if (numbers != "") {
                    string[] parts = numbers.Replace("(","").Replace(")", "").Split(',');
                    line = int.Parse(parts[0]);
                    column = int.Parse(parts[1]);
                }
                if (type == "ERROR") {
                    this.BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "CCSError", InputFile, line, column, line, column+1, message, "", ""));
                } else if (type == "WARNING") {
                    this.BuildEngine.LogWarningEvent(new BuildWarningEventArgs("", "CCSError", InputFile, line, column, line, column+1, message, "", ""));
                }
            } else {
                this.BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "CCSError", InputFile, 0, 0,0, 0, "Unexpected message from compiler: " + singleLine, "", ""));
            }
            
        }

        protected override string GenerateFullPathToTool() {
            return Path.Combine(Environment.GetEnvironmentVariable("CCS_PATH"), "ccs.exe");
        }

        protected override string ToolName {
            get { return "CCS Compiler"; }
        }
    }
}
