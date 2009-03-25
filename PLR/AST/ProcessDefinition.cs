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
            _processConstructors.Add(this.ProcessConstant.Name, constructor);
        }

        public override void Compile(CompileInfo info) {
            info.Type = (TypeBuilder)info.Module.GetType(this.ProcessConstant.Name);
            Type baseType = typeof(ProcessBase);

            MethodBuilder methodStart = info.Type.DefineMethod("RunProcess", MethodAttributes.Public | MethodAttributes.Virtual);
            info.Type.DefineMethodOverride(methodStart, baseType.GetMethod("RunProcess"));
            info.ILGenerator = methodStart.GetILGenerator();

            Call(new ThisPointer(typeof(ProcessBase)), "InitSetID", true).Compile(info);

            
            info.ILGenerator.BeginExceptionBlock();
            this.Process.Compile(info);
            info.ILGenerator.BeginCatchBlock(typeof(ProcessKilledException));
            info.ILGenerator.Emit(OpCodes.Pop); //Pop the exception off the stack
            EmitDebug("Caught ProcessKilledException", info);
            //Just catch here to abort, don't do anything
            info.ILGenerator.EndExceptionBlock();

            //Ev
            Call(new ThisPointer(typeof(ProcessBase)), "Die", true).Compile(info);
            info.ILGenerator.Emit(OpCodes.Ret);
            info.Type.CreateType();
        }
    }
}
