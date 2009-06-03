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
using PLR.AST.Actions;
using PLR.AST.Expressions;
using PLR.Runtime;
using PLR.Compilation;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Processes {

    public class ActionPrefix : Process {

        public ActionPrefix(Action action, Process proc) {
            _children.Add(action);
            _children.Add(proc);
        }

        public Action Action {
            get { return (Action) _children[2]; }
            set { _children[2] = value; }
        }

        public Process Process {
            get { return (Process)_children[3];}
            set {  _children[3] = value; }
        }

        protected override bool WrapInTryCatch {
            get { return true; } 
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            Action.Compile(context);
            TypeInfo inner = null;
            if (Process.HasRestrictionsOrPreProcess) {
                inner = Process.CompileNewProcessStart(context, "Inner");
            }
            this.Process.Compile(context);
            if (inner != null) {
                this.Process.CompileNewProcessEnd(context);
                EmitRunProcess(context, inner, true, null, true);
            }
        }

        public override string ToString() {
            return Action.ToString() + " . " + Process.ToString();
        }

    }
}

