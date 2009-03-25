using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;


namespace PLR.AST.ActionHandling {
    public class RelabelActions : PreProcessActions{
        public void Add(ActionID from, ActionID to) {
            _children.Add(from);
            _children.Add(to);
        }
        public ActionID this[ActionID val] {
            get {
                for (int i = 0; i < _children.Count; i += 2) {
                    if (_children[i].Equals(val)) {
                        return (ActionID)_children[i + 1];
                    }
                }
                return null;
            }
        }

        public override IEnumerator<Node> GetEnumerator() {
            List<Node> keys = new List<Node>();
            for (int i = 0; i < _children.Count; i += 2) {
                keys.Add(_children[i]);
            }

            return keys.GetEnumerator();
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileInfo info) {
            if (_children.Count > 0) {
                MethodInfo method = typeof(ProcessBase).GetMethod("PreProcess");
                MethodBuilder relabel = info.Type.DefineMethod("RelabelAction", MethodAttributes.Public | MethodAttributes.Virtual, typeof(IAction), new Type[] { typeof(IAction) });
                info.PreProcess = relabel;
                ILGenerator il = relabel.GetILGenerator();
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Isinst, typeof(ChannelSync));
                Label isChannelSync = il.DefineLabel();
                il.Emit(OpCodes.Brtrue, isChannelSync);

                ////If not ChannelSync then return the passed in argument
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(isChannelSync);

                LocalBuilder channel = il.DeclareLocal(typeof(ChannelSync));
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, typeof(ChannelSync));
                il.Emit(OpCodes.Stloc, channel);
                Label end = il.DefineLabel();
                //Now we've got a ChannelSync object
                for (int i = 0; i < _children.Count; i+= 2) {
                    ActionID key = (ActionID) _children[i];
                    ActionID value = (ActionID) _children[i+1];
                    
                    //Check whether the key matches the current name
                    il.Emit(OpCodes.Ldloc, channel);
                    il.Emit(OpCodes.Call, typeof(ChannelSync).GetMethod("get_Name"));
                    il.Emit(OpCodes.Ldstr, key.Name);
                    il.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality"));
                    Label nextCheck = il.DefineLabel();

                    il.Emit(OpCodes.Brfalse, nextCheck);
                    il.Emit(OpCodes.Ldloc, channel);
                    il.Emit(OpCodes.Ldstr, value.Name);
                    il.Emit(OpCodes.Call, typeof(ChannelSync).GetMethod("set_Name"));
                    il.Emit(OpCodes.Br, end);
                    il.MarkLabel(nextCheck);
                }
                il.MarkLabel(end);
                il.Emit(OpCodes.Ldloc, channel);
                il.Emit(OpCodes.Ret);
                info.Type.DefineMethodOverride(relabel, method);
            }
        }
    }
}
