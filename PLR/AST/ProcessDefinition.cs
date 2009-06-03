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
using PLR.AST.Interfaces;

public struct Foo {
    public object a;
    public object c;

}
public class Bar {
    Foo f;
    public Bar(Foo f) {
        this.f = f;
    }
}
namespace PLR.AST {

    public class ProcessDefinition : Node, IVariableAssignment {


        protected string _name;
        public String Name {
            get { return _name; }
            set { _name = value; }
        }

        protected ExpressionList _expressions = new ExpressionList();

        public ExpressionList Variables {
            get { return _expressions; }
        }

        protected Process _proc;
        public Process Process {
            get { return _proc; }
            set { _proc = value; }
        }

        protected bool _entryProc;
        public bool EntryProc {
            get { return _entryProc; }
            set { _entryProc = value; }
        }


        public ProcessDefinition(Process proc, string name, bool entryProc) {
            _name = name;
            _proc = proc;
            _entryProc = entryProc;
            _children.Add(_expressions);
            _children.Add(proc);
        }


        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            visitor.Visit((IVariableAssignment)this);
        }


        private class VariableCollection : AbstractVisitor {
            public List<string> vars = new List<string>();
            public override void Visit(Variable var) {
                if (!vars.Contains(var.Name)) {
                    vars.Add(var.Name);
                }
            }
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
            if (_expressions.Count > 0) {

                foreach (Variable v in this.Variables) {
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
                foreach (Variable v in _expressions) {
                    list.Add(v);
                }
                return list;
            }
        }

        #endregion
    }
}
