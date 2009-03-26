using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;
using PLR.Runtime;


namespace PLR.AST.ActionHandling {
    public class RelabelActions : PreProcessActions{
        private Dictionary<string, string> _mapping = new Dictionary<string, string>();
        public void Add(string fromName, string toName) {
            _mapping.Add(fromName, toName);
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            if (_mapping.Count > 0) {
                MethodBuilder relabel = context.Type.DefineMethod("RelabelAction", MethodAttributes.Public | MethodAttributes.Static, typeof(IAction), new Type[] { typeof(IAction) });
                context.PreProcess = relabel;
                ILGenerator il = relabel.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Isinst, typeof(ChannelSyncAction));
                Label isChannelSync = il.DefineLabel();
                il.Emit(OpCodes.Brtrue, isChannelSync);

                ////If not ChannelSync then return the passed in argument
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(isChannelSync);

                LocalBuilder channel = il.DeclareLocal(typeof(ChannelSyncAction));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, typeof(ChannelSyncAction));
                il.Emit(OpCodes.Stloc, channel);
                Label end = il.DefineLabel();
                //Now we've got a ChannelSync object
                foreach (string from in _mapping.Keys) {
                    string to = _mapping[from];                    
                    //Check whether the key matches the current name
                    il.Emit(OpCodes.Ldloc, channel);
                    il.Emit(OpCodes.Call, typeof(ChannelSyncAction).GetMethod("get_Name"));
                    il.Emit(OpCodes.Ldstr, from);
                    il.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality"));
                    Label nextCheck = il.DefineLabel();

                    il.Emit(OpCodes.Brfalse, nextCheck);
                    il.Emit(OpCodes.Ldloc, channel);
                    il.Emit(OpCodes.Ldstr, to);
                    il.Emit(OpCodes.Call, typeof(ChannelSyncAction).GetMethod("set_Name"));
                    il.Emit(OpCodes.Br, end);
                    il.MarkLabel(nextCheck);
                }
                il.MarkLabel(end);
                il.Emit(OpCodes.Ldloc, channel);
                il.Emit(OpCodes.Ret);
                base.OverridePreProcess(context, relabel);
            }
        }
    }
}
