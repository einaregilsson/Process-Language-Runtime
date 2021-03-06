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
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;
using PLR.Runtime;
using PLR.AST.Expressions;

namespace PLR.AST.Processes {

    public class ParallelComposition : Process{

        public List<Process> Processes {
            get {
                List<Process> list = new List<Process>();
                for (int i = 2; i < _children.Count; i++) {
                    list.Add((Process)_children[i]);
                }
                return list;
            }
        }

        public override List<Process> FlowsTo {
            get {
                return this.Processes;
            }
        }

        public void Add(Process p) {
            _children.Add(p);
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public override void Compile(CompileContext context) {

            List<Process> processes = this.Processes;

            for (int i = 0; i < processes.Count; i++) {
                Process p = processes[i];
                string innerTypeName = "Parallel" + (i + 1);

                if (context.Options.Optimize && !p.IsUsed) {
                    continue;
                }
                TypeInfo newProcType = null;
                if (p.HasRestrictionsOrPreProcess || !(p is ProcessConstant)) {
                    newProcType = p.CompileNewProcessStart(context, innerTypeName);
                }
                p.Compile(context);

                if (newProcType != null) {
                    p.CompileNewProcessEnd(context);
                    EmitRunProcess(context, newProcType, false, p.LexicalInfo, true);
                }
            }
        }

        public override string ToString() {
            return Util.Join(" | ", _children);
        }
    }
}
