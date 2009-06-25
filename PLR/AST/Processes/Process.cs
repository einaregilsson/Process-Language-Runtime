/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
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

        public Process() {
            _children.Add(null); //PreProcessActions
            _children.Add(null); //Restrict
        }

        public virtual List<Process> FlowsTo { get { return new List<Process>(); } }
        public virtual List<Variable> ReadVariables { get { return new List<Variable>(); } }
        public virtual List<Variable> AssignedVariables { get { return new List<Variable>(); } }

        public PreProcessActions PreProcessActions {
            get { return (PreProcessActions) _children[0]; }
            set { _children[0] = value; }
        }

        public ActionRestrictions ActionRestrictions {
            get { return (ActionRestrictions)_children[1]; }
            set { _children[1] = value; }
        }

        protected virtual bool WrapInTryCatch {
            get { return false; } //Most processes won't need a try catch around them.
        }

        protected void EmitRunProcess(CompileContext context, TypeInfo procType, bool setGuidOnProc, LexicalInfo lexInfo, bool loadVariables) {
            if (context.Type == null || context.ILGenerator == null) {
                return; //Are at top level and so can't run the process
            }
            ILGenerator il = context.ILGenerator;


            if (loadVariables) {
                foreach (string paramName in procType.ConstructorParameters) {
                    il.Emit(OpCodes.Ldloc, context.Type.GetLocal(paramName));
                }
            }
            LocalBuilder loc = il.DeclareLocal(typeof(ProcessBase));
            il.Emit(OpCodes.Newobj, procType.Constructor);
            il.Emit(OpCodes.Stloc, loc);
            il.Emit(OpCodes.Ldloc, loc);
            il.Emit(OpCodes.Ldarg_0); //load the "this" pointer

            //The current process doesn't have a restrict or relabel method, no reason for it
            //to continue living, set the parent process of the new proc as our own parent process
            if (!context.Type.IsPreProcessed && !context.Type.IsRestricted && !context.Type.MustLiveOn) {
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

        private class VariableCollection : AbstractVisitor {
            public List<Variable> Variables = new List<Variable>();
            public override void Visit(Variable var) {
                if (!Variables.Contains(var)) {
                    Variables.Add(var);
                }
            }
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        /// <summary>
        /// Compiles the start block of
        /// </summary>
        /// <param name="context"></param>
        public TypeInfo CompileNewProcessStart(CompileContext context, string name) {
            try {
                //Get this before we push a new type on the stack, we are going to need it...
                Dictionary<string, LocalBuilder> currentLocals = null;

                if (context.Type != null) {
                    currentLocals = context.Type.Locals;
                }

                if (context.CurrentMasterType != null) { //Are in a type, so let's create a nested one
                    TypeInfo nestedType = new TypeInfo();
                    nestedType.Builder = context.Type.Builder.DefineNestedType(name, TypeAttributes.NestedPublic | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, typeof(ProcessBase));
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

                    VariableCollection col = new VariableCollection();
                    col.Start(this);
                    List<Type> paramTypes = new List<Type>();
                    List<Variable> constructorParams = new List<Variable>();
                    foreach (Variable v in col.Variables) {
                        if (currentLocals != null && currentLocals.ContainsKey(v.Name)) { //We have defined this local variable and so should pass it to the new process
                            paramTypes.Add(typeof(object));
                            constructorParams.Add(v);
                            context.Type.ConstructorParameters.Add(v.Name); //Needed to pass the right parameters along later
                            context.Type.Fields.Add(context.Type.Builder.DefineField(v.Name, typeof(object), FieldAttributes.Private));
                        }
                    }
                    context.Type.Constructor = context.Type.Builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramTypes.ToArray());
                    ILGenerator ilCon = context.Type.Constructor.GetILGenerator();
                    ilCon.Emit(OpCodes.Ldarg_0);
                    ilCon.Emit(OpCodes.Call, typeof(ProcessBase).GetConstructor(new Type[] { }));

                    for (int i = 0; i < constructorParams.Count; i++) {
                        context.Type.Constructor.DefineParameter(i + 1, ParameterAttributes.None, constructorParams[i].Name);
                        //save the variables argument we got passed in
                        ilCon.Emit(OpCodes.Ldarg_0);
                        ilCon.Emit(OpCodes.Ldarg, i + 1);
                        ilCon.Emit(OpCodes.Stfld, context.Type.GetField(constructorParams[i].Name));
                    }
                    ilCon.Emit(OpCodes.Ret);
                }

                //Do this after the constructor has been defined, because the fields
                //of the object are defined in the constructor...
                foreach (FieldBuilder field in context.Type.Fields) {
                    LocalBuilder local = context.ILGenerator.DeclareLocal(typeof(object));
                    if (context.Options.Debug) {
                        local.SetLocalSymInfo(field.Name);
                    }
                    context.Type.Locals.Add(field.Name, local);
                    //Add the field value to the local variable, so we can concentrate on only local vars
                    context.ILGenerator.Emit(OpCodes.Ldarg_0);
                    context.ILGenerator.Emit(OpCodes.Ldfld, field);
                    context.ILGenerator.Emit(OpCodes.Stloc, local);

                }

                return context.Type;
            } catch (Exception) {
                return null;
            }
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
