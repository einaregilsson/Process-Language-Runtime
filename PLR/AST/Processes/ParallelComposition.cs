using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;
using PLR.Runtime;
using PLR.AST.Expressions;

namespace PLR.AST.Processes {

    public class ParallelComposition : Process{

        public new Process this[int index] {
            get { return (Process) _children[index]; }
        }
        
        public void Add(Process p) {
            _children.Add(p);
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            base.Compile(context);
            TypeBuilder enclosingType = context.Type;
            ILGenerator originalIL = context.ILGenerator;
            List<Type> subTypes = new List<Type>();
            List<ConstructorBuilder> cons = new List<ConstructorBuilder>();
            AppDomain.CurrentDomain.TypeResolve += new ResolveEventHandler(CurrentDomain_TypeResolve);

            for (int i = 0; i < this.Count; i++) {
                Process p = this[i];
                context.Type = enclosingType.DefineNestedType("Parallel" + (i+1), TypeAttributes.NestedPublic | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, typeof(ProcessBase));
                Type baseType = typeof(ProcessBase);

                MethodBuilder methodStart = context.Type.DefineMethod("RunProcess", MethodAttributes.Public | MethodAttributes.Virtual);
                context.Type.DefineMethodOverride(methodStart, baseType.GetMethod("RunProcess"));
                context.ILGenerator = methodStart.GetILGenerator();

                Call(new ThisPointer(typeof(ProcessBase)), "InitSetID", true).Compile(context);

                context.ILGenerator.BeginExceptionBlock();
                p.Compile(context);
                context.ILGenerator.BeginCatchBlock(typeof(ProcessKilledException));
                context.ILGenerator.Emit(OpCodes.Pop); //Pop the exception off the stack
                EmitDebug("Caught ProcessKilledException", context);
                //Just catch here to abort, don't do anything
                context.ILGenerator.EndExceptionBlock();

                Call(new ThisPointer(typeof(ProcessBase)), "Die", true).Compile(context);
                context.ILGenerator.Emit(OpCodes.Ret);
                cons.Add(context.Type.DefineDefaultConstructor(MethodAttributes.Public));
                subTypes.Add(context.Type.CreateType());
            }
            context.Type = enclosingType;
            context.ILGenerator = originalIL;
            f = context;
            //Now start all the new processes
            for (int i = 0; i < subTypes.Count; i++) {
                Type subType = subTypes[i];
                ConstructorBuilder c = cons[i];
                originalIL.Emit(OpCodes.Newobj, c);
                Call(new ThisPointer(typeof(ProcessBase)), "Run", true).Compile(context);
            }
        }
        private CompileContext f;

        Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args) {
            string s = args.Name;
            return f.Module.Assembly;
        }

    }
}
