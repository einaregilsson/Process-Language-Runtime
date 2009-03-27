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
            ConstructorBuilder inner = CheckIfNeedNewProcess(context, false);

            for (int i = 0; i < this.Count; i++) {
                Process p = this[i];
                p.NestedProcess = true;
                string innerTypeName = "Parallel" + (i + 1);
                p.TypeName = innerTypeName;
                p.Compile(context); //Compiling a nested inner process will also start it
                string fullname = context.Type.FullName + "+" + innerTypeName;
                ConstructorBuilder con;
                //Start the processes if they were created under this name.
                if (context.NamedProcessConstructors.TryGetValue(fullname, out con)) {
                    EmitRunProcess(context, con);
                }
            }
            
            if (inner != null) {
                CompileNewProcessEnd(context, false);
                EmitRunProcess(context, inner);
            }
        }
    }
}
