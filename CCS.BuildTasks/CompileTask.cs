﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CCS.BuildTasks {
    public class CompileTask : Task {

        public override bool Execute() {
            this.BuildEngine.LogErrorEvent(new BuildErrorEventArgs("FOO", "ASDF", "john", 3, 4, 5, 6, "HELLO", "", ""));
            return true;
        }
    }
}
