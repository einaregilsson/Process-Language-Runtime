using System.Collections.Generic;
using PLR.AST.Processes;
using PLR.AST.Expressions;
using PLR.Compilation;
using System;
using System.Reflection.Emit;
using System.Reflection;
using PLR.Runtime;

namespace PLR.AST {

    public class ProcessDefinition : Node {


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

            //Find all variables used in this process...
            VariableCollection varCollection = new VariableCollection();
            varCollection.Start(this);

            //Define the variables type...
            TypeBuilder variables = type.DefineNestedType("Variables", TypeAttributes.Class | TypeAttributes.BeforeFieldInit | TypeAttributes.NestedFamily);
            newTypeInfo.Variables = variables;
            newTypeInfo.VariablesConstructor = variables.DefineDefaultConstructor(MethodAttributes.Assembly);
            newTypeInfo.VariablesField = type.DefineField("_variables", newTypeInfo.Variables, FieldAttributes.Private);

            foreach (string variableName in varCollection.vars) {
                FieldBuilder field = variables.DefineField(variableName, typeof(int), FieldAttributes.Assembly);
                newTypeInfo.AddField(field);
            }

            Type[] paramTypes = new Type[this.Variables.Count];
            for (int i = 0; i < paramTypes.Length; i++) {
                paramTypes[i] = typeof(int);
            }
            constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramTypes);
            ILGenerator ilCon = constructor.GetILGenerator();
            ConstructorInfo conBase = typeof(ProcessBase).GetConstructor(new Type[] { });
            ilCon.Emit(OpCodes.Ldarg_0);
            ilCon.Emit(OpCodes.Call, conBase);

            //Create a new variables object and save it to field
            ilCon.Emit(OpCodes.Newobj, newTypeInfo.VariablesConstructor);
            ilCon.Emit(OpCodes.Ldarg_0);
            ilCon.Emit(OpCodes.Stfld, newTypeInfo.VariablesField);
            
            //For every variable in the constructor, set it on the variables object
            for (int i = 0; i < this.Variables.Count; i++) {
                Variable var = (Variable)this.Variables[i];
                ilCon.Emit(OpCodes.Ldarg, i + 1);
                ilCon.Emit(OpCodes.Ldfld, newTypeInfo.VariablesField);
                ilCon.Emit(OpCodes.Stfld, newTypeInfo.GetField(var.Name));
            }
            ilCon.Emit(OpCodes.Ret);

            newTypeInfo.Constructor = constructor;
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
            ConstructorBuilder inner = this.Process.CompileNewProcessStart(context, this.FullName);
            this.Process.Compile(context);
            this.Process.CompileNewProcessEnd(context);
        }
    }
}
