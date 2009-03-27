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


        public void CompileSignature(ModuleBuilder module, CompileContext context) {
            TypeBuilder type = module.DefineType(this.ProcessConstant.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, typeof(ProcessBase));
            ConstructorBuilder constructor = type.DefineDefaultConstructor(MethodAttributes.Public);
            context.NamedProcessConstructors.Add(this.ProcessConstant.Name, constructor);
        }

        public override void Compile(CompileContext context) {
            ConstructorBuilder inner = this.Process.CompileNewProcessStart(context, this.ProcessConstant.Name);
            this.Process.Compile(context);
            this.Process.CompileNewProcessEnd(context);
        }
    }
}
