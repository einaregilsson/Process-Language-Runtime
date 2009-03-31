using System.Collections.Generic;
using System.Reflection.Emit;
using PLR.Compilation;
using PLR.Runtime;

namespace PLR.AST.Processes {

    public class NonDeterministicChoice : Process {

        public new Process this[int index] {
            get { return (Process)_children[index]; }
        }

        public void Add(Process p) {
            _children.Add(p);
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {

            for (int i = 0; i < this.Count; i++) {
                Process p = this[i];
                if (p is ProcessConstant && !p.HasRestrictionsOrPreProcess) {
                    //Don't need to create a special proc just for wrapping this
                    ProcessConstant pc = (ProcessConstant)p;
                    EmitRunProcess(context, context.NamedProcessConstructors[pc.Name], true, p.LexicalInfo);
                } else {
                    string innerTypeName = "NonDeterministic" + (i + 1);
                    ConstructorBuilder con = p.CompileNewProcessStart(context, innerTypeName);
                    p.Compile(context);
                    p.CompileNewProcessEnd(context);
                    EmitRunProcess(context, con, true, p.LexicalInfo);
                }
            }
        }
    }
}
