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
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            ConstructorBuilder inner = CheckIfNeedNewProcess(context, false);

            for (int i = 0; i < this.Count; i++) {
                Process p = this[i];
                p.NestedProcess = true;
                string innerTypeName = "NonDeterministic" + (i + 1);
                p.TypeName = innerTypeName;
                p.Compile(context); //Compiling a nested inner process will also start it
                string fullname = context.Type.FullName + "+" + innerTypeName;
                ConstructorBuilder con;
                //Start the processes if they were created under this name.
                if (context.NamedProcessConstructors.TryGetValue(fullname, out con)) {
                    context.ILGenerator.Emit(OpCodes.Newobj, con);
                    LocalBuilder nd = context.ILGenerator.DeclareLocal(typeof(ProcessBase));
                    context.ILGenerator.Emit(OpCodes.Stloc, nd);

                    context.ILGenerator.Emit(OpCodes.Ldloc, nd);
                    context.ILGenerator.Emit(OpCodes.Ldarg_0);
                    context.ILGenerator.Emit(OpCodes.Call, typeof(ProcessBase).GetMethod("get_SetID"));
                    context.ILGenerator.Emit(OpCodes.Call, typeof(ProcessBase).GetMethod("set_SetID"));
                    context.ILGenerator.Emit(OpCodes.Ldloc, nd);
                    context.ILGenerator.Emit(OpCodes.Call, typeof(ProcessBase).GetMethod("Run"));
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
