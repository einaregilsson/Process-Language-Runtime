using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.ActionHandling {
    public class ChannelRestrictions : ActionRestrictions{
        public void Add(ActionID action) {
            _children.Add(action);
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileInfo info) {
            if (_children.Count > 0) {
                MethodInfo method = typeof(ProcessBase).GetMethod("IsRestricted");
                MethodBuilder restrict = info.Type.DefineMethod("IsRestricted", MethodAttributes.Public | MethodAttributes.Virtual, typeof(bool), new Type[] { typeof(IAction) });
                ILGenerator il = restrict.GetILGenerator();
                info.Restrict = restrict;
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Isinst, typeof(ChannelSync));
                Label isChannelSync = il.DefineLabel();
                il.Emit(OpCodes.Brtrue, isChannelSync);

                ////If not ChannelSync then return false
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(isChannelSync);

                LocalBuilder channel = il.DeclareLocal(typeof(ChannelSync));
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, typeof(ChannelSync));
                il.Emit(OpCodes.Stloc, channel);

                //Now we've got a ChannelSync object
                foreach (ActionID id in _children) {
                    il.Emit(OpCodes.Ldloc, channel);
                    il.Emit(OpCodes.Call, typeof(ChannelSync).GetMethod("get_Name"));
                    il.Emit(OpCodes.Ldstr, id.Name);
                    il.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality"));
                }
                //Or them all together
                for (int i = 0; i < _children.Count - 1; i++) {
                    il.Emit(OpCodes.Or);
                }

                il.Emit(OpCodes.Ret);
                info.Type.DefineMethodOverride(restrict, method);
            }
        }
    }
}
