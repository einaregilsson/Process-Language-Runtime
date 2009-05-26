/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Expressions {

    public class Bool : BooleanExpression {
        private bool _value;
        public Bool(bool value) {
            _value = value;
        }
        public bool Value {
            get { return _value; }
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            if (_value) {
                context.ILGenerator.Emit(OpCodes.Ldc_I4_1);
            } else {
                context.ILGenerator.Emit(OpCodes.Ldc_I4_0);
            }
        }

        public override string ToString() {
            return _value ? "true" : "false";
        }
        public override System.Type Type {
            get { return typeof(bool); }
        }
    }
}
