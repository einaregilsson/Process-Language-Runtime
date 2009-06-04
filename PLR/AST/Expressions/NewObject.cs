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

namespace PLR.Analysis.Expressions {
    public class NewObject : MethodInvokeBase {

        private ConstructorInfo _constructor;
        public NewObject(Type objectType, params object[] args) : base(args) {
            _constructor = objectType.GetConstructor(GetArgTypes());
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public override Type Type {
            get { return _constructor.DeclaringType; }
        }

        public override string ToString() {
            return "new " + _constructor.DeclaringType.Name;
        }

        public override void Compile(CompileContext context) {
            foreach (Expression exp in ChildNodes) {
                exp.Compile(context);
            }
            context.ILGenerator.Emit(OpCodes.Newobj, _constructor);
        }
    }
}
