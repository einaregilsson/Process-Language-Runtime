/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using PLR.Compilation;
using PLR.AST.Expressions;
using PLR.Runtime;

namespace PLR.AST.Actions
{
    public class OutAction : Action
    {
        public OutAction(string name) : base(Regex.Replace(name, "^_|_$","")) { }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public void AddExpression(Expression exp) {
            _children.Add(exp);
        }

        public override List<Variable> ReadVariables {
            get { return FindReadVariables(this); }
        }

        public override void Compile(CompileContext context) {
            Type procType = typeof(ProcessBase);
            ILGenerator il = context.ILGenerator;

            EmitDebug("Preparing to sync now...", context);
            LocalBuilder syncObject = il.DeclareLocal(typeof(ChannelSyncAction));

            il.Emit(OpCodes.Ldarg_0); //this
            il.Emit(OpCodes.Ldstr, Name);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, _children.Count);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newobj, typeof(ChannelSyncAction).GetConstructor(new Type[] { typeof(string), typeof(ProcessBase), typeof(int), typeof(bool) }));
            il.Emit(OpCodes.Stloc, syncObject);

            //Now put the result of any expression we have into the sync objects
            foreach (Expression exp in _children) {
                il.Emit(OpCodes.Ldloc, syncObject);
                exp.Compile(context);
                if (exp is ArithmeticExpression || exp is MethodCallExpression) {
                    il.Emit(OpCodes.Box, typeof(int));
                }
                il.Emit(OpCodes.Call, typeof(ChannelSyncAction).GetMethod("AddValue"));
            }
            
            il.Emit(OpCodes.Ldloc, syncObject);
            il.Emit(OpCodes.Call, SyncMethod);
            //Do nothing here after. In an action class that actually does something we would
            //compile it here.
            context.MarkSequencePoint(this.LexicalInfo);

        }

        public override string ToString() {
            return "_" + _name + "_";
        }
    }
}
