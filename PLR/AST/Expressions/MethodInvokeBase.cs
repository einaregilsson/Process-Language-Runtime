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

namespace PLR.AST.Expressions {
    public abstract class MethodInvokeBase : Expression {

        public MethodInvokeBase(params object[] args) {
            for (int i = 0; i < args.Length; i++) {
                if (args[i] is int) {
                    _children.Add(new Number((int)args[i]));
                } else if (args[i] is bool) {
                    _children.Add(new Bool((bool)args[i]));
                } else if (args[i] is string) {
                    _children.Add(new PLRString((string)args[i]));
                } else if (args[i] is LocalBuilder) {
                    _children.Add(new Variable((LocalBuilder)args[i]));
                } else if (args[i] is Expression) {
                    _children.Add((Expression) args[i]);
                }
            }
        }

        protected Type[] GetArgTypes() {
            Type[] types = new Type[_children.Count];
            for (int i = 0; i < _children.Count; i++) {
                types[i] = ((Expression)_children[i]).Type;
            }
            return types;
        }
    }
}
