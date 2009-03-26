using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Processes {

    public class ParallelComposition : Process{

        public new Process this[int index] {
            get { return (Process) _children[index]; }
        }
        
        public void Add(Process p) {
            _children.Add(p);
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            base.Compile(context);
            foreach (Process p in this) {
                p.Compile(context);
            }
        }

    }
}
