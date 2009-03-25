using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {
    public class NewObject : MethodInvokeBase {

        private ConstructorInfo _constructor;
        public NewObject(Type objectType, params object[] args) : base(args) {
            _constructor = objectType.GetConstructor(GetArgTypes());
        }

        public override Type Type {
            get { return _constructor.DeclaringType; }
        }

        public override void Compile(CompileInfo info) {
            foreach (Expression exp in Arguments) {
                exp.Compile(info);
            }
            info.ILGenerator.Emit(OpCodes.Newobj, _constructor);
        }
    }
}
