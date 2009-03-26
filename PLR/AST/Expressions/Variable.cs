﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace PLR.AST.Expressions {
    public class Variable : ArithmeticExpression {
        protected string _name;
        private LocalBuilder _local;
        private static int _genCounter = 0;
        public string Name { get { return _name; } }
        public Variable(string name) {
            _name = name;
        }
        public Variable(LocalBuilder local) {
            _name = "AutoGenerated" + _genCounter++;
            _local = local;
        }

        public override int Value {
            get { throw new NotSupportedException("Variable value not available at compile time!"); }
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override Type Type {
            get {
                return _local.LocalType;
            }
        }
        #region Compilation
        public virtual void Declare(ILGenerator il) {
            if (_local != null) {
                throw new InvalidOperationException("Variable " + _name + " is already declared!");
            }
            _local = il.DeclareLocal(this.Type);
            _local.SetLocalSymInfo(_name);
        }

        public virtual void AssignTo(ILGenerator il) {
            il.Emit(OpCodes.Stloc, _local);
        }

        public override string ToString() {
            return _name;
        }

        public override void Compile(CompileContext context) {
            context.ILGenerator.Emit(OpCodes.Ldloc, _local);
        }
        #endregion
    }
}
