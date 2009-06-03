/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Expressions {

    public class PLRString : Expression {
        public static bool DisplayWithoutQuotes { get; set; }
        private string _value;
        public PLRString(string value) {
            _value = value;
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public override void Compile(CompileContext context) {
            context.ILGenerator.Emit(OpCodes.Ldstr, _value);
        }

        public string Value {
            get { return _value; }
        }
        public override string ToString() {
            if (PLRString.DisplayWithoutQuotes) {
                return _value;
            } else {
                return "\"" + _value + "\"";
            }
        }

        public override Type Type {
            get { return typeof(string); }
        }
    }
}
