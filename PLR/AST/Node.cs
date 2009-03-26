﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PLR.AST.Expressions;

namespace PLR.AST {

    public abstract class Node : IEnumerable<Node> {

        public abstract void Accept(AbstractVisitor visitor);
        //Source file contextrmation

        protected static Dictionary<string, ConstructorBuilder> _processConstructors = new Dictionary<string, ConstructorBuilder>();
        protected object _extraData;
        public object ExtraData {
            get { return _extraData; }
            set { _extraData = value; }
        }

        protected int _line;
        public int Line
        {
            get { return _line; }
            set { _line = value; }
        }

        protected int _col;
        public int Column
        {
            get { return _col; }
            set { _col = value; }
        }

        protected int _pos;
        public int Position
        {
            get { return _pos; }
            set { _pos = value; }
        }

        protected int _length;
        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }

        protected int _parenCount;
        public int ParenCount
        {
            get { return _parenCount; }
            set { _parenCount = value; }
        }

        public bool HasParens { get { return ParenCount > 0; } }

        protected List<Node> _children = new List<Node>();
        public void SetPos(int line, int col, int length, int pos)
        {
            _line = line;
            _col = col;
            _length = length;
            _pos = pos;
        }

        //protected static Formatter formatter = new Formatter();

        public virtual int Count
        {
            get { return _children.Count; }
        }
        public virtual List<Node> ChildNodes {
            get { return _children; }
        }

        public Node this[int index]{
            get
            {
                return _children[index];
            }
        }

        public override string ToString() {
            return null;
            //return formatter.Format(this);
        }

        public virtual IEnumerator<Node> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #region Compile helpers

        public abstract void Compile(CompileContext context);

        protected void Assign(LocalBuilder local, Expression exp, CompileContext context) {
            exp.Compile(context);
            context.ILGenerator.Emit(OpCodes.Stloc, local);
        }

        protected MethodCallExpression Call(LocalBuilder instance, string methodName, bool popReturn, params object[] args) {
            MethodCallExpression mc = new MethodCallExpression(instance, methodName, args);
            mc.PopReturnValue = popReturn;
            return mc;
        }
        
        protected MethodCallExpression Call(Expression instance, string methodName, bool popReturn, params object[] args) {
            MethodCallExpression mc = new MethodCallExpression(instance, methodName, args);
            mc.PopReturnValue = popReturn;
            return mc;
        }

        protected MethodCallExpression Call(Type type, string methodName, bool popReturn, params object[] args) {
            MethodCallExpression mc = new MethodCallExpression(type, methodName, args);
            mc.PopReturnValue = popReturn;
            return mc;
        }


        public NewObject New(Type type, params object[] args) {
            return new NewObject(type, args);
        }

        protected void EmitDebug(string msg, CompileContext context) {
            ThisPointer thisP = new ThisPointer(typeof(ProcessBase));
            Call(thisP, "Debug", true, msg).Compile(context);
        }
        protected void CallScheduler(string methodName, bool popReturn, CompileContext context, params object[] args) {
            Call(Call(typeof(Scheduler), "get_Instance", false), methodName, popReturn, args).Compile(context);
        }
            

        #endregion


    }
}
