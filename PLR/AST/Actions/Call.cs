using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using PLR.AST.Expressions;
using PLR.Compilation;
using PLR.Runtime;

namespace PLR.AST.Actions {
    public class Call : Action{
        private MethodCallExpression _exp;
        
        public Call(MethodCallExpression callExpr)
            : base(callExpr.ToString()) {
            _exp = callExpr;
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
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
            _exp.Compile(context);
        }

    }
}
