using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Processes {
    public class NilProcess : Process{
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            EmitDebug("Turned into 0",context);
            if (context.Options.Debug) {
                context.MarkSequencePoint(LexicalInfo);
                context.ILGenerator.Emit(OpCodes.Nop);
            }
        }

        public override string ToString() {
            return "0";
        }
    }
}
