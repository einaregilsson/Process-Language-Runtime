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

        protected void EmitRunProcess(CompileContext context, ConstructorBuilder con, bool setGuidOnProc, LexicalInfo lexInfo, bool loadVariables) {
            if (context.Type == null || context.ILGenerator == null) {
                return; //Are at top level and so can't run the process
            }
            ILGenerator il = context.ILGenerator;

            LocalBuilder loc = il.DeclareLocal(typeof(ProcessBase));

            if (context.Type.VariablesField != null && loadVariables) {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, context.Type.VariablesField);
            }

            il.Emit(OpCodes.Newobj, con);
            il.Emit(OpCodes.Stloc, loc);
            il.Emit(OpCodes.Ldloc, loc);
            il.Emit(OpCodes.Ldarg_0); //load the "this" pointer

            //The current process doesn't have a restrict or relabel method, no reason for it
            //to continue living, set the parent process of the new proc as our own parent process
            if (!context.Type.IsPreProcessed && !context.Type.IsRestricted) {
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

            if (context.CurrentMasterType != null) { //Are in a type, so let's create a nested one
                TypeInfo nestedType = new TypeInfo();
                nestedType.Builder = context.CurrentMasterType.Builder.DefineNestedType(name, TypeAttributes.NestedPublic | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, typeof(ProcessBase));
                context.PushType(nestedType);
            } else {
                context.PushType(context.GetType(name));
                context.CurrentMasterType = context.Type;
            }
            Type baseType = typeof(ProcessBase);

            if (this.PreProcessActions != null) {
                this.PreProcessActions.Compile(context);
                context.Type.IsPreProcessed = true;
            }
            if (this.ActionRestrictions != null) {
                this.ActionRestrictions.Compile(context);
                context.Type.IsRestricted = true;
            }

            MethodBuilder methodStart = context.Type.Builder.DefineMethod("RunProcess", MethodAttributes.Public | MethodAttributes.Virtual);
            context.Type.Builder.DefineMethodOverride(methodStart, baseType.GetMethod("RunProcess"));
            context.PushIL(methodStart.GetILGenerator());
            Call(new ThisPointer(typeof(ProcessBase)), "InitSetID", true).Compile(context);
            if (this.WrapInTryCatch) {
                context.ILGenerator.BeginExceptionBlock();
            }

            if (context.Type.Constructor == null) { //Nested type which hasn't defined its constructor yet
                if (context.CurrentMasterType.Variables != null) {
                    context.Type.Constructor = context.Type.Builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { context.CurrentMasterType.Variables });
                    context.Type.VariablesField = context.Type.Builder.DefineField("_variables", context.CurrentMasterType.Variables, FieldAttributes.Private);
                    ILGenerator ilCon = context.Type.Constructor.GetILGenerator();
                    ilCon.Emit(OpCodes.Ldarg_0);
                    ilCon.Emit(OpCodes.Call, typeof(ProcessBase).GetConstructor(new Type[] { }));

                    //save the variables argument we got passed in
                    ilCon.Emit(OpCodes.Ldarg_0);
                    ilCon.Emit(OpCodes.Ldarg_1);
                    ilCon.Emit(OpCodes.Stfld, context.Type.VariablesField);
                    ilCon.Emit(OpCodes.Ret);
                } else {
                    context.Type.Constructor = context.Type.Builder.DefineDefaultConstructor(MethodAttributes.Public);
                }
            }
            return context.Type.Constructor;
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
            context.Type.Builder.CreateType();
            context.PopType();
            context.PopIL();
        }
    }
}
