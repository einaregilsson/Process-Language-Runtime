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
using PLR.Analysis.Expressions;
using PLR.Compilation;
using PLR.Runtime;

namespace PLR.Analysis.ActionHandling {
    public class ChannelRestrictions : ActionRestrictions{
        private List<string> _channelNames = new List<string>();
        public List<string> ChannelNames { get { return _channelNames; } }
        public void Add(string channelName) {
            _channelNames.Add(channelName);
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public override void Compile(CompileContext context) {
            if (_channelNames.Count > 0) {
                MethodBuilder restrict = context.Type.Builder.DefineMethod("RestrictByName", MethodAttributes.Public | MethodAttributes.Static, typeof(bool), new Type[] { typeof(IAction) });
                ILGenerator il = restrict.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Isinst, typeof(ChannelSyncAction));
                Label isChannelSync = il.DefineLabel();
                il.Emit(OpCodes.Brtrue, isChannelSync);

                ////If not ChannelSync then return false
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(isChannelSync);

                LocalBuilder channel = il.DeclareLocal(typeof(ChannelSyncAction));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, typeof(ChannelSyncAction));
                il.Emit(OpCodes.Stloc, channel);

                //Now we've got a ChannelSync object
                foreach (string channelName in _channelNames) {
                    il.Emit(OpCodes.Ldloc, channel);
                    il.Emit(OpCodes.Call, typeof(ChannelSyncAction).GetMethod("get_Name"));
                    il.Emit(OpCodes.Ldstr, channelName);
                    il.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality"));
                }
                //Or them all together
                for (int i = 0; i < _channelNames.Count - 1; i++) {
                    il.Emit(OpCodes.Or);
                }

                il.Emit(OpCodes.Ret);
                base.OverrideRestrict(context, restrict);
            }
        }
    }
}
