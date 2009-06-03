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
using PLR.AST.Expressions;
using PLR.Compilation;
using PLR.Runtime;
using PLR.AST.Interfaces;

namespace PLR.AST.Actions {
    public class Call : Action, IVariableReader{
        
        public Call(MethodCallExpression callExpr)
            : base(callExpr.ToString()) {
            _children.Add(callExpr);
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            visitor.Visit((IVariableReader)this);
        }

        public List<Variable> ReadVariables {
            get { return FindReadVariables(this.MethodCallExpr); }
        }

        public MethodCallExpression MethodCallExpr {
            get { return (MethodCallExpression)_children[0]; }
        }

        public override void Compile(CompileContext context) {
            Type procType = typeof(ProcessBase);
            ILGenerator il = context.ILGenerator;

            EmitDebug("Preparing to sync now...", context);

            il.Emit(OpCodes.Ldarg_0); //this, to call the Sync

            //Create methodcall sync object
            il.Emit(OpCodes.Ldstr, this.Name);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, typeof(MethodCallAction).GetConstructor(new Type[] { typeof(string), typeof(ProcessBase)}));

            //Call sync
            il.Emit(OpCodes.Call, SyncMethod);
            context.MarkSequencePoint(this.LexicalInfo);
            //Now compile the actual method call
            MethodCallExpr.Compile(context);
        }

    }
}
