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
            args += " /out:" + OutputFile + " ";
            args += InputFile;
            return args;        
        }

        public override bool Execute() {
            bool result = base.Execute();
            this.BuildEngine.LogErrorEvent(new BuildErrorEventArgs(this.Debug + this.OutputFile + this.InputFile, "ASDF", "john", 3, 4, 5, 6, "Done", "", ""));
            return true;
        }

        protected override string GenerateFullPathToTool() {
            return @"D:\dtu\msc\build\ccs.exe";
        }

        protected override string ToolName {
            get { return "CCS Compiler"; }
        }
    }
}
