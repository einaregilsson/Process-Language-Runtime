using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;
using PLR.Runtime;
using PLR.AST.Expressions;

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

            for (int i = 0; i < this.Count; i++) {
                Process p = this[i];
                string innerTypeName = "Parallel" + (i + 1);

                ConstructorBuilder con = null;
                if (p.HasRestrictionsOrPreProcess || !(p is ProcessConstant)) {
                    con = p.CompileNewProcessStart(context, innerTypeName);
                }
                p.Compile(context); 
                
                if (con != null) {
                    p.CompileNewProcessEnd(context);
                    EmitRunProcess(context, con,false, p.LexicalInfo, true);
                }
            }
        }

        public override string ToString() {
            return Util.Join(" | ", _children);
        }
    }
}
