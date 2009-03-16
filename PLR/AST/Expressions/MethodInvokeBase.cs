using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {
    public abstract class MethodInvokeBase : Expression {
        private Expression[] _args;
        protected Expression[] Arguments { get { return _args; } }

        public MethodInvokeBase(params object[] args) {
            _args = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++) {
                if (args[i] is int) {
                    _args[i] = new Number((int)args[i]);
                } else if (args[i] is bool) {
                    _args[i] = new Bool((bool)args[i]);
                } else if (args[i] is string) {
                    _args[i] = new PLRString((string)args[i]);
                } else if (args[i] is LocalBuilder) {
                    _args[i] = new Variable((LocalBuilder)args[i]);
                } else if (args[i] is Expression) {
                    _args[i] = (Expression) args[i];
                }
            }
        }

        protected Type[] GetArgTypes() {
            Type[] types = new Type[_args.Length];
            for (int i = 0; i < _args.Length; i++) {
                types[i] = _args[i].Type;
            }
            return types;
        }
    }
}
