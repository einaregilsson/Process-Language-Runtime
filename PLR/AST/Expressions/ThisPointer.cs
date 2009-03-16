using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {
    public class ThisPointer : Expression{
        private Type _thisType;

        public ThisPointer(Type thisType) {
            _thisType = thisType;
        }
        public override Type Type {
            get { return _thisType; }
        }

        public override void Compile(ILGenerator il) {
            il.Emit(OpCodes.Ldarg_0);
        }
    }
}
