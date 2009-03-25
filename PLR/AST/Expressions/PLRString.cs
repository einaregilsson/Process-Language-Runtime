using System;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {

    public class PLRString : Expression {
        private string _value;
        public PLRString(string value) {
            _value = value;
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileInfo info) {
            info.ILGenerator.Emit(OpCodes.Ldstr, _value);
        }

        public override Type Type {
            get { return typeof(string); }
        }
    }
}
