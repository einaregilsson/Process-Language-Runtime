using System.Collections.Generic;
using PLR.AST.Processes;
using PLR.AST.Expressions;
using PLR.Compilation;
using System;
using System.Reflection.Emit;
using System.Reflection;
using PLR.Runtime;

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

        public override void Compile(CompileContext context) {
            context.Type = (TypeBuilder)context.Module.GetType(this.ProcessConstant.Name);
            Type baseType = typeof(ProcessBase);

            MethodBuilder methodStart = context.Type.DefineMethod("RunProcess", MethodAttributes.Public | MethodAttributes.Virtual);
            context.Type.DefineMethodOverride(methodStart, baseType.GetMethod("RunProcess"));
            context.ILGenerator = methodStart.GetILGenerator();

            Call(new ThisPointer(typeof(ProcessBase)), "InitSetID", true).Compile(context);

            
            context.ILGenerator.BeginExceptionBlock();
            this.Process.Compile(context);
            context.ILGenerator.BeginCatchBlock(typeof(ProcessKilledException));
            context.ILGenerator.Emit(OpCodes.Pop); //Pop the exception off the stack
            EmitDebug("Caught ProcessKilledException", context);
            //Just catch here to abort, don't do anything
            context.ILGenerator.EndExceptionBlock();

            //Ev
            Call(new ThisPointer(typeof(ProcessBase)), "Die", true).Compile(context);
            context.ILGenerator.Emit(OpCodes.Ret);
            context.Type.CreateType();
        }
    }
}
