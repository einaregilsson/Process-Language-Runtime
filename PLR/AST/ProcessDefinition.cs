/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System.Collections.Generic;
using PLR.AST.Processes;
using PLR.AST.Expressions;
using PLR.Compilation;
using System;
using System.Reflection.Emit;
using System.Reflection;
using PLR.Runtime;

namespace PLR.AST {

    public class ProcessDefinition : Node {

        public String Name { get; set;}

        public ExpressionList Variables {
            get { return (ExpressionList)_children[0]; }
        }

        public Process Process {
            get { return (Process) _children[1]; }
            set { _children[1] = value; }
        }

        public bool EntryProc {get;set;}

        public ProcessDefinition(Process proc, string name, bool entryProc) {
            Name = name;
            EntryProc = entryProc;
            _children.Add(new ExpressionList());
            _children.Add(proc);
        }


        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public void CompileSignature(ModuleBuilder module, CompileContext context) {
            string name = this.Name;
            if (this.Variables.Count > 0) {
                name += "_" + this.Variables.Count;
            }

            TypeInfo newTypeInfo = new TypeInfo();
            TypeBuilder type = module.DefineType(name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, typeof(ProcessBase));
            ConstructorBuilder constructor;
            newTypeInfo.Builder = type;

            //If there are variables then define a type for them ...
            if (this.Variables.Count > 0) {

                foreach (Variable v in this.Variables) {
                    if (context.Options.Optimize && !v.IsUsed) {
                        continue;
                    }
                    FieldBuilder field = type.DefineField(v.Name, typeof(object), FieldAttributes.Assembly);
                    newTypeInfo.AddField(field);
                }
                
                Type[] paramTypes = new Type[this.Variables.Count];
                for (int i = 0; i < paramTypes.Length; i++) {
                    paramTypes[i] = typeof(object);
                }
                constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramTypes);
                ILGenerator ilCon = constructor.GetILGenerator();
                ConstructorInfo conBase = typeof(ProcessBase).GetConstructor(new Type[] { });
                ilCon.Emit(OpCodes.Ldarg_0);
                ilCon.Emit(OpCodes.Call, conBase);

                //For every variable in the constructor, set it on the corresponding field
                for (int i = 0; i < this.Variables.Count; i++) {
                    Variable var = (Variable)this.Variables[i];
                    constructor.DefineParameter(i + 1, ParameterAttributes.None, var.Name);
                    newTypeInfo.ConstructorParameters.Add(var.Name);

                    if (context.Options.Optimize && !var.IsUsed) {
                        continue;
                    }
                    ilCon.Emit(OpCodes.Ldarg_0);
                    ilCon.Emit(OpCodes.Ldarg, i + 1);
                    ilCon.Emit(OpCodes.Stfld, newTypeInfo.GetField(var.Name));
                }
                ilCon.Emit(OpCodes.Ret);
                newTypeInfo.Constructor = constructor;
            } else {
                newTypeInfo.Constructor = type.DefineDefaultConstructor(MethodAttributes.Public);
            }
            
            context.AddType(newTypeInfo);
        }

        public string FullName {
            get {
                if (this.Variables.Count > 0) {
                    return this.Name + "_" + this.Variables.Count;
                }
                return this.Name;
            }
        }

        public override void Compile(CompileContext context) {
            this.Process.CompileNewProcessStart(context, this.FullName);
            this.Process.Compile(context);
            this.Process.CompileNewProcessEnd(context);
        }

        #region IVariableAssignment Members

        public List<Variable> AssignedVariables {
            get {
                List<Variable> list = new List<Variable>();
                foreach (Variable v in Variables) {
                    list.Add(v);
                }
                return list;
            }
        }

        #endregion
    }
}
