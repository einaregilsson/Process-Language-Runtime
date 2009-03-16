using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {
    public class TypedNull : Expression {

        private Type _nullType;
        public TypedNull(Type nullType) {
            _nullType = nullType;
        }
        public override Type Type {
            get { return _nullType; }
        }

        public override void Compile(ILGenerator il) {
            il.Emit(OpCodes.Ldnull);
        }
    }
}
