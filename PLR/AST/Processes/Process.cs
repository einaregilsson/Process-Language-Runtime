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

        //A typename for the compiled process, null for processes that are nameless
        private string _typeName = null;
        public string TypeName{
            get { return _typeName; }
            set { _typeName = value; }
        }

        //Whether or not the process should be compiled as a nested process
        private bool _nestedProcess = true;
        public bool NestedProcess {
            get { return _nestedProcess; }
            set { _nestedProcess = value; }
        }

        private ActionRestrictions _restrictions = null;
        public ActionRestrictions ActionRestrictions { 
            get { return _restrictions; }
            set { _restrictions = value; }
        }

        protected ConstructorBuilder CheckIfNeedNewProcess(CompileContext context, bool wrapInTryCatch) {
            if (HasRestrictionsOrPreProcess || this.TypeName != null) { //We need a new process
                return CompileNewProcessStart(context, this.TypeName ?? "Inner", wrapInTryCatch);
            }
            return null;
        }

        protected void EmitRunProcess(CompileContext context, ConstructorBuilder con) {
            if (context.Type == null || context.ILGenerator == null) {
                return; //Are at top level and so can't run the process
            }
            context.ILGenerator.Emit(OpCodes.Newobj, con);
            context.ILGenerator.Emit(OpCodes.Call, typeof(ProcessBase).GetMethod("Run"));
        }

        /// <summary>
        /// Compiles the start block of
        /// </summary>
        /// <param name="context"></param>
        public ConstructorBuilder CompileNewProcessStart(CompileContext context, string name, bool wrapInTryCatch) {
            
            if (this.NestedProcess) { //No, so let's create a nested type
                context.PushType(context.Type.DefineNestedType(name, TypeAttributes.NestedPublic | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, typeof(ProcessBase)));
            } else {
                context.PushType((TypeBuilder)context.Module.GetType(name));
            }
            Type baseType = typeof(ProcessBase);

            if (this.PreProcessActions != null) {
                this.PreProcessActions.Compile(context);
            }
            if (this.ActionRestrictions != null) {
                this.ActionRestrictions.Compile(context);
            }

            MethodBuilder methodStart = context.Type.DefineMethod("RunProcess", MethodAttributes.Public | MethodAttributes.Virtual);
            context.Type.DefineMethodOverride(methodStart, baseType.GetMethod("RunProcess"));
            context.PushIL(methodStart.GetILGenerator());
            Call(new ThisPointer(typeof(ProcessBase)), "InitSetID", true).Compile(context);
            if (wrapInTryCatch) {
                context.ILGenerator.BeginExceptionBlock();
            }

            if (!context.NamedProcessConstructors.ContainsKey(context.Type.Name)) {
                context.NamedProcessConstructors.Add(context.Type.FullName, context.Type.DefineDefaultConstructor(MethodAttributes.Public));
            }
            return context.NamedProcessConstructors[context.Type.FullName];
        }

        protected bool HasRestrictionsOrPreProcess {
            get { return this.ActionRestrictions != null || this.PreProcessActions != null; }
        }


        public void CompileNewProcessEnd(CompileContext context, bool wrapInTryCatch) {
            if (wrapInTryCatch) {
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
