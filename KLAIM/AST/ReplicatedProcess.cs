using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PLR.AST;
using PLR.AST.Processes;
using PLR.Compilation;
using System.Reflection.Emit;
using System.Threading;

namespace KLAIM.AST {

    public class ReplicatedProcess : Process{

        private int _maxCount;
        public ReplicatedProcess(Process p, int maxCount) {
            _children.Add(p);
            _maxCount = maxCount;
        }

        public Process Process {
            get { return (Process)  _children[2]; }
            set { _children[0] = value;}
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this); 
        }

        public override void Compile(CompileContext context) {
            string innerTypeName = "Repl_" + context.CurrentMasterType.Name;

            ConstructorBuilder con = null;
            con = Process.CompileNewProcessStart(context, innerTypeName);
            Process.Compile(context);
            Process.CompileNewProcessEnd(context);
            Label loopStart = context.ILGenerator.DefineLabel();
            context.ILGenerator.MarkLabel(loopStart);
            for (int i = 0; i < _maxCount; i++) {
                EmitRunProcess(context, con, false, Process.LexicalInfo, true);
                context.ILGenerator.Emit(OpCodes.Ldc_I4, 100);
                context.ILGenerator.Emit(OpCodes.Call, typeof(System.Threading.Thread).GetMethod("Sleep", new Type[] {typeof(int)}));
                context.ILGenerator.Emit(OpCodes.Br, loopStart);
            }
        }
    }
}
