using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using PLR.AST;
using PLR.AST.Expressions;

namespace PLR.AST.Processes
{
    public class ProcessConstant : Process {
        public ProcessConstant(string name) {
            _name = name;
            _subscript = new Subscript();
            _children.Add(Subscript);
        }

        private string _name;
        public string Name { get { return _name; } }

        protected Subscript _subscript;
        public Subscript Subscript { get { return _subscript; } }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is ProcessConstant))
            {
                return false;
            }
            ProcessConstant other = (ProcessConstant)obj;
            return other.Name == this.Name && other.Subscript.Count == this.Subscript.Count;
        }

        public override int GetHashCode() {
            return (this.Name + this.Subscript.Count).GetHashCode();

        }

        public override void Compile(CompileInfo info) {
            //Invoke a new instance of that process

            ConstructorBuilder constructor = _processConstructors[this.Name];
            ILGenerator il = info.ILGenerator;
            LocalBuilder loc = il.DeclareLocal(typeof(ProcessBase));
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Stloc, loc);

            il.Emit(OpCodes.Ldloc, loc);
            il.Emit(OpCodes.Ldarg_0); //load the "this" pointer

            if (info.Restrict == null && info.PreProcess == null) {
                //The current process doesn't have a restrict or relabel method, no reason for it
                //to continue living, set the parent process of the new proc as our own parent process
                il.Emit(OpCodes.Call, MethodResolver.GetMethod(typeof(ProcessBase), "get_Parent"));
            }
            il.Emit(OpCodes.Call, MethodResolver.GetMethod(typeof(ProcessBase), "set_Parent"));

            //Run the new proc
            il.Emit(OpCodes.Ldloc, loc);
            Call(typeof(ProcessBase), "Run", true).Compile(info);
        }
    }
}
