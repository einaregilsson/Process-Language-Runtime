using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using PLR.Compilation;
using PLR.Runtime;
using PLR.AST.Expressions;

namespace PLR.AST.Actions {
    public class InAction : Action {
        public InAction(string name) : base(name) { }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public void AddVariable(Variable var) {
            _children.Add(var);
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
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Newobj, typeof(ChannelSyncAction).GetConstructor(new Type[] { typeof(string), typeof(ProcessBase), typeof(int), typeof(bool) }));
            il.Emit(OpCodes.Stloc, syncObject);
            il.Emit(OpCodes.Ldloc, syncObject);
            il.Emit(OpCodes.Call, SyncMethod);
            context.MarkSequencePoint(this.LexicalInfo);

            //Save values to variables
            for (int i = 0; i < _children.Count; i++) {
                Variable var = (Variable) _children[i];

                //Load the variables field on the current process
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, context.Type.VariablesField);

                //Get the value to assign to it...
                il.Emit(OpCodes.Ldloc, syncObject);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Call, typeof(ChannelSyncAction).GetMethod("GetValue"));
                il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new Type[] {typeof(object)}));
                //...and assign it
                il.Emit(OpCodes.Stfld, context.CurrentMasterType.GetField(var.Name));
            }
        }
    }
}
