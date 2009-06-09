/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Expressions {
    public class TypedNull : Expression {

        private Type _nullType;
        public TypedNull(Type nullType) {
            _nullType = nullType;
        }
        public override Type Type {
            get { return _nullType; }
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public override string ToString() {
            return "null";
        }

        public override void Compile(CompileContext context) {
            context.ILGenerator.Emit(OpCodes.Ldnull);
        }
    }
}
