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
    public class MethodCallExpression : MethodInvokeBase{
        private bool _popReturnValue = true;
        private Expression _instance;
        private MethodInfo _method;
        private string _methodName = null;

        public MethodCallExpression(LocalBuilder instance, string methodName, params object[] args) : base(args) {
            _instance = new Variable(instance);
            _methodName = methodName;
            LookupMethod(instance.LocalType, methodName);
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public string MethodName {
            get { return _methodName; }
        }
        public MethodCallExpression(Expression instance, string methodName, params object[] args) : base(args) {
            _instance = instance;
            _methodName = methodName;
            LookupMethod(instance.Type, methodName);
        }

        //Static
        public MethodCallExpression(Type objectType, string methodName, params object[] args) : base(args) {
            LookupMethod(objectType, methodName);
        }

        //Static, ooked up at compile time
        public MethodCallExpression(string methodName, params object[] args)
            : base(args) {
            _methodName = methodName;
        }

        private void LookupMethod(Type objectType, string name) {
            _method = objectType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, GetArgTypes(), null);
            if (_method == null) {
                Console.Error.WriteLine("ERROR: Method '{0}' was not found!");
                System.Environment.Exit(1);
            }
        }

        public override string ToString() {
            return _methodName + "(" + Util.Join(", ", ChildNodes) + ")";
        }

        public override Type Type {
            get { return _method == null ? typeof(int) : _method.ReturnType; } //Assume int for unknowns, we mostly operate on ints
        }

        public bool PopReturnValue {
            get { return _popReturnValue; }
            set { _popReturnValue = value; }
        }

        public override void Compile(CompileContext context) {
            Type[] argtypes = GetArgTypes();

            if (_method == null) {
                _method = context.GetMethod(_methodName, argtypes);
            }

            if (_instance != null) {
                _instance.Compile(context);
            }

            ParameterInfo[] actualParams = _method.GetParameters();
            for (int i = 0 ; i< ChildNodes.Count; i++) {
                Expression exp = (Expression)ChildNodes[i];
                exp.Compile(context);
                if (exp.Type.Equals(typeof(int))
                    && actualParams[i].ParameterType.Equals(typeof(object))) {
                    context.ILGenerator.Emit(OpCodes.Box, typeof(int));
                }
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
