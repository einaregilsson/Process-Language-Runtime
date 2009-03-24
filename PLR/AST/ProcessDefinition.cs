using System.Collections.Generic;
using PLR.AST.Processes;
using PLR.AST.Expressions;
using System;
using System.Reflection.Emit;
using System.Reflection;

namespace PLR.AST {

    public class ProcessDefinition : Node{

        protected ProcessConstant _procConst;
        public ProcessConstant ProcessConstant
        {
            get { return _procConst; }
            set { _procConst = value; }
        }

        protected Process _proc;
        public Process Process
        {
            get { return _proc; }
            set { _proc = value; }
        }

        protected bool _entryProc;
        public bool EntryProc
        {
            get { return _entryProc; }
            set { _entryProc = value; }
        }


        public ProcessDefinition(ProcessConstant pconst, Process proc, bool entryProc)
        {
            _proc = proc;
            _procConst = pconst;
            _entryProc = entryProc;
            _children.Add(pconst);
            _children.Add(proc);
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }


        public void CompileSignature(ModuleBuilder module) {
            TypeBuilder type = module.DefineType(this.ProcessConstant.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, typeof(ProcessBase));
            ConstructorBuilder constructor = type.DefineDefaultConstructor(MethodAttributes.Public);
            _processConstructors.Add(type, constructor);
        }

        public void Compile(ModuleBuilder module) {
            TypeBuilder type = (TypeBuilder)module.GetType(this.ProcessConstant.Name);
            Type baseType = typeof(ProcessBase);

            MethodBuilder methodStart = type.DefineMethod("RunProcess", MethodAttributes.Public | MethodAttributes.Virtual);
            type.DefineMethodOverride(methodStart, baseType.GetMethod("RunProcess"));
            ILGenerator il = methodStart.GetILGenerator();

            Call(new ThisPointer(typeof(ProcessBase)), "InitSetID", true).Compile(il);
            CompileInfo info = new CompileInfo();
            info.Module = module;
            info.ILGenerator = il;
            info.Type = type;
            this.Process.Compile(info);

            Call(new ThisPointer(typeof(ProcessBase)), "Die", true).Compile(il);
            il.Emit(OpCodes.Ret);
            type.CreateType();
        }
    }
}
