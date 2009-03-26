using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {
    public class MethodCall : MethodInvokeBase{
        private bool _popReturnValue = true;
        private Expression _instance;
        private MethodInfo _method;

        public MethodCall(LocalBuilder instance, string methodName, params object[] args) : base(args) {
            _instance = new Variable(instance);
            LookupMethod(instance.LocalType, methodName);
        }

        public MethodCall(Expression instance, string methodName, params object[] args) : base(args) {
            _instance = instance;
            LookupMethod(instance.Type, methodName);
        }

        //Static
        public MethodCall(Type objectType, string methodName, params object[] args) : base(args) {
            LookupMethod(objectType, methodName);
        }

        private void LookupMethod(Type objectType, string name) {
            _method = objectType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, GetArgTypes(), null);
            if (_method == null) {
                _method = objectType.BaseType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, GetArgTypes(), null);
            }
        }

        public override Type Type {
            get { return _method.ReturnType; }
        }

        public bool PopReturnValue {
            get { return _popReturnValue; }
            set { _popReturnValue = value; }
        }

        public override void Compile(CompileContext context) {
            if (_instance != null) {
                _instance.Compile(context);
            }
            
            foreach (Expression exp in Arguments) {
                exp.Compile(context);
            }
            if (_method.IsVirtual) {
                context.ILGenerator.Emit(OpCodes.Callvirt, _method);
            } else {
                context.ILGenerator.Emit(OpCodes.Call, _method);
            }
            if (PopReturnValue && _method.ReturnType != typeof(void)) {
                context.ILGenerator.Emit(OpCodes.Pop);
            }
        }
    }
}
