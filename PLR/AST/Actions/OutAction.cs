using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace PLR.AST.Actions
{
    public class OutAction : Action
    {
        public OutAction(string name) : base(Regex.Replace(name, "^_|_$","")) { }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            Type procType = typeof(ProcessBase);
            ILGenerator il = context.ILGenerator;

            EmitDebug("Preparing to sync now...", context);

            il.Emit(OpCodes.Ldarg_0); //this
            il.Emit(OpCodes.Ldstr, Name);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Newobj, typeof(ChannelSyncAction).GetConstructor(new Type[] { typeof(string), typeof(ProcessBase), typeof(bool) }));
            il.Emit(OpCodes.Call, SyncMethod);
            //Do nothing here after. In an action class that actually does something we would
            //compile it here.
        }
    }
}
