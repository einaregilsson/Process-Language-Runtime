using System.Reflection.Emit;

namespace PLR.AST.Processes {
    public class NilProcess : Process{
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Compile(CompileInfo info) {
            EmitDebug("Turned into 0",info);
        }

    }
}
