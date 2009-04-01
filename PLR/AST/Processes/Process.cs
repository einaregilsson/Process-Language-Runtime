using PLR.AST;
using PLR.AST.Actions;
using PLR.Runtime;
using PLR.AST.ActionHandling;
using PLR.AST.Expressions;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;
using System;

namespace PLR.AST.Processes {
    public abstract class Process : Node {
        private PreProcessActions _preprocess = null;
        public PreProcessActions PreProcessActions {
            get { return _preprocess; }
            set { _preprocess = value; }
        }


        private ActionRestrictions _restrictions = null;
        public ActionRestrictions ActionRestrictions { 
            get { return _restrictions; }
            set { _restrictions = value; }
        }

        protected virtual bool WrapInTryCatch {
            get { return false; } //Most processes won't need a try catch around them.
        }

        protected void EmitRunProcess(CompileContext context, ConstructorBuilder con, bool setGuidOnProc, LexicalInfo lexInfo) {
            if (context.Type == null || context.ILGenerator == null) {
                return; //Are at top level and so can't run the process
            }
            ILGenerator il = context.ILGenerator;

            LocalBuilder loc = il.DeclareLocal(typeof(ProcessBase));
            il.Emit(OpCodes.Newobj, con);
            il.Emit(OpCodes.Stloc, loc);
            
            //Set fields on the newly created process
            foreach (FieldBuilder sourceField in context.GetFields().Values) {
                FieldBuilder destField = context.GetField(con.DeclaringType.FullName, sourceField.Name);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, sourceField);
                il.Emit(OpCodes.Ldloc, loc);
                il.Emit(OpCodes.Stfld, destField);
            }

            il.Emit(OpCodes.Ldloc, loc);
            il.Emit(OpCodes.Ldarg_0); //load the "this" pointer

            //The current process doesn't have a restrict or relabel method, no reason for it
            //to continue living, set the parent process of the new proc as our own parent process
            if (!context.IsRestricted(context.Type)) {
                il.Emit(OpCodes.Call, MethodResolver.GetMethod(typeof(ProcessBase), "get_Parent"));
            }
            il.Emit(OpCodes.Call, MethodResolver.GetMethod(typeof(ProcessBase), "set_Parent"));

            if (setGuidOnProc) {
                il.Emit(OpCodes.Ldloc, loc);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof(ProcessBase).GetMethod("get_SetID"));
                il.Emit(OpCodes.Call, typeof(ProcessBase).GetMethod("set_SetID"));
            }

            il.Emit(OpCodes.Ldloc, loc);
            if (context.Options.Debug && lexInfo != null) {
                //context.MarkSequencePoint(lexInfo);
            }
            il.Emit(OpCodes.Call, MethodResolver.GetMethod(typeof(ProcessBase), "Run"));
        }


        /// <summary>
        /// Compiles the start block of
        /// </summary>
        /// <param name="context"></param>
        public ConstructorBuilder CompileNewProcessStart(CompileContext context, string name) {

            if (context.Type != null) { //Are in a type, so let's create a nested one
                context.PushType(context.Type.DefineNestedType(name, TypeAttributes.NestedPublic | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, typeof(ProcessBase)));
            } else {
                context.PushType((TypeBuilder)context.Module.GetType(name));
            }
            Type baseType = typeof(ProcessBase);

            if (this.PreProcessActions != null) {
                this.PreProcessActions.Compile(context);
                context.AddRestrictedType(context.Type);
            }
            if (this.ActionRestrictions != null) {
                this.ActionRestrictions.Compile(context);
                context.AddRestrictedType(context.Type);
            }

            MethodBuilder methodStart = context.Type.DefineMethod("RunProcess", MethodAttributes.Public | MethodAttributes.Virtual);
            context.Type.DefineMethodOverride(methodStart, baseType.GetMethod("RunProcess"));
            context.PushIL(methodStart.GetILGenerator());
            Call(new ThisPointer(typeof(ProcessBase)), "InitSetID", true).Compile(context);
            if (this.WrapInTryCatch) {
                context.ILGenerator.BeginExceptionBlock();
            }

            if (!context.NamedProcessConstructors.ContainsKey(context.Type.Name)) {
                context.NamedProcessConstructors.Add(context.Type.FullName, context.Type.DefineDefaultConstructor(MethodAttributes.Public));
                foreach (FieldBuilder field in context.ProcessFields.Values) {
                    FieldBuilder newField = context.Type.DefineField(field.Name, field.FieldType, FieldAttributes.Public);
                    context.AddField(context.Type.FullName, newField);
                }
            }
            return context.NamedProcessConstructors[context.Type.FullName];
        }

        internal bool HasRestrictionsOrPreProcess {
            get { return this.ActionRestrictions != null || this.PreProcessActions != null; }
        }


        public void CompileNewProcessEnd(CompileContext context) {
            if (this.WrapInTryCatch) {
                context.ILGenerator.BeginCatchBlock(typeof(ProcessKilledException));
                context.ILGenerator.Emit(OpCodes.Pop); //Pop the exception off the stack
                EmitDebug("Caught ProcessKilledException", context);
                //Just catch here to abort, don't do anything
                context.ILGenerator.EndExceptionBlock();
            }

            Call(new ThisPointer(typeof(ProcessBase)), "Die", true).Compile(context);
            context.ILGenerator.Emit(OpCodes.Ret);
            context.Type.CreateType();
            context.PopType();
            context.PopIL();
        }
    }
}
