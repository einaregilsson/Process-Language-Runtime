/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System.Collections.Generic;
using System.Reflection.Emit;
using PLR.Compilation;
using PLR.Runtime;

namespace PLR.AST.Processes {

    public class NonDeterministicChoice : Process {

        public void Add(Process p) {
            _children.Add(p);
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public List<Process> Processes {
            get {
                List<Process> list = new List<Process>();
                for (int i = 2; i < _children.Count; i++) {
                    list.Add((Process)_children[i]);
                }
                return list;
            }
        }
        public override void Compile(CompileContext context) {

            for (int i = 0; i < this.Processes.Count; i++) { //First two items are ActionRestriction and PreProcess
                Process p = (Process) this.Processes[i];
                if (p is ProcessConstant && !p.HasRestrictionsOrPreProcess) {
                    //Don't need to create a special proc just for wrapping this
                    ProcessConstant pc = (ProcessConstant)p;
                    EmitRunProcess(context, context.GetType(pc.Name).Constructor, true, p.LexicalInfo, true);
                } else {
                    string innerTypeName = "NonDeterministic" + (i + 1);
                    ConstructorBuilder con = p.CompileNewProcessStart(context, innerTypeName);
                    p.Compile(context);
                    p.CompileNewProcessEnd(context);
                    EmitRunProcess(context, con, true, p.LexicalInfo, true);
                }
            }
        }

        public override string ToString() {
            return Util.Join(" + ", _children);
        }

    }
}
