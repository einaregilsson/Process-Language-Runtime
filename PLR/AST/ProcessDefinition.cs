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
            ConstructorBuilder constructor;
            if (this.ProcessConstant.Subscript.Count > 0) {
                Type[] paramTypes = new Type[this.ProcessConstant.Subscript.Count];
                for (int i = 0; i < paramTypes.Length; i++) {
                    paramTypes[i] = typeof(int);
                }
                constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramTypes);
                ILGenerator ilCon = constructor.GetILGenerator();
                ConstructorInfo conBase = typeof(ProcessBase).GetConstructor(new Type[] { });
                ilCon.Emit(OpCodes.Ldarg_0);
                ilCon.Emit(OpCodes.Call, conBase);
                for (int i = 0; i < this.ProcessConstant.Subscript.Count; i++) {
                    ArithmeticExpression exp = this.ProcessConstant.Subscript[i];
                    if (!(exp is Variable)) {
                        throw new Exception("Process constants can only have variables");
                    }
                    Variable var = (Variable)exp;
                    FieldBuilder field = type.DefineField(var.Name, typeof(int), FieldAttributes.Public);
                    ilCon.Emit(OpCodes.Ldarg_0);
                    ilCon.Emit(OpCodes.Ldarg, i + 1);
                    ilCon.Emit(OpCodes.Stfld, field);
                    context.AddField(context.ProcessName, field);
                }
                ilCon.Emit(OpCodes.Ret);
            } else {
                constructor = type.DefineDefaultConstructor(MethodAttributes.Public);
            }
            
            context.NamedProcessConstructors.Add(this.ProcessConstant.Name, constructor);
        }

        public override void Compile(CompileContext context) {
            ConstructorBuilder inner = this.Process.CompileNewProcessStart(context, this.ProcessConstant.Name);
            this.Process.Compile(context);
            this.Process.CompileNewProcessEnd(context);
        }
    }
}
