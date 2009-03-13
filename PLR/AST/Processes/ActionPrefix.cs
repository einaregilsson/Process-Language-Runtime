﻿using System;
using System.Collections.Generic;
using PLR.AST.Actions;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Processes {

    public class ActionPrefix : Process{

        public ActionPrefix(Action action, Process proc) {
            _action = action;
            _proc = proc;
            _children.Add(Action);
            _children.Add(proc);
        }

        private Action _action;
        public Action Action
        {
            get { return _action; }
            set { _action = value; _children[0] = _action; }
        }

        private Process _proc;
        public Process Process {
            get { return _proc; }
            set { _proc = value; _children[1] = _proc; }
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override void Compile(ILGenerator il) {
            Type baseType = typeof(ProcessBase);
            il.Emit(OpCodes.Ldstr, _action.Name);
            il.Emit(OpCodes.Ldarg_0); //Push the "this" onto the stack
            il.Emit(_action is InAction ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newobj, MethodResolver.GetConstructor(typeof(StringAction)));
            il.EmitCall(OpCodes.Call, MethodResolver.GetMethod(baseType, "Sync"), new Type[] {typeof(StringAction)});
            il.Emit(OpCodes.Pop);
        }
    }
}

