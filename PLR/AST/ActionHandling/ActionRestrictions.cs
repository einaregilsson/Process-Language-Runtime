using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using PLR.Compilation;
using PLR.Runtime;


namespace PLR.AST.ActionHandling {
    public abstract class ActionRestrictions : Node {
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        protected void OverrideRestrict(CompileContext context, MethodInfo newMethod) {
            MethodInfo baseGetRestrict = typeof(ProcessBase).GetMethod("get_Restrict");
            MethodBuilder getRestrict = context.Type.DefineMethod("get_Restrict", MethodAttributes.Public | MethodAttributes.Virtual, typeof(RestrictAction), new Type[] { });

            ILGenerator il = getRestrict.GetILGenerator();
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldftn, newMethod);
            il.Emit(OpCodes.Newobj, typeof(RestrictAction).GetConstructors()[0]);
            il.Emit(OpCodes.Ret);
            context.Type.DefineMethodOverride(getRestrict, baseGetRestrict);
        }

    }
}
