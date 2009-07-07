/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PLR.AST.Expressions;
using PLR.Compilation;
using PLR.Runtime;

namespace PLR.AST {

    public abstract class Node : IEnumerable<Node> {


        public Node() {
            IsUsed = true;
        }
        public virtual void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }
        //Source file contextrmation
        
        protected readonly LexicalInfo _lexInfo = new LexicalInfo();
        public LexicalInfo LexicalInfo {
            get { return _lexInfo; }
        }

        public void Replace(Node oldChild, Node newChild) {
            int pos = this.ChildNodes.IndexOf(oldChild);
            if (pos == -1) {
                throw new Exception("Parameter 'oldChild' is not a child of this node!");
            }
            this.ChildNodes[pos] = newChild;
        }

        public object Tag { get; set; }
        public Node Parent { get; set;}
        public int ParenCount {get;set;}
        public bool IsUsed { get; set; }

        public bool HasParens { get { return ParenCount > 0; } }
        protected List<Node> _children = new List<Node>();

        public virtual int Count
        {
            get { return _children.Count; }
        }
        public virtual List<Node> ChildNodes {
            get { return _children; }
        }

        public Node this[int index]{
            get {
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


        private class VariableCollection : AbstractVisitor {
            public List<Variable> Variables = new List<Variable>();
            public override void Visit(Variable var) {
                if (!Variables.Contains(var)) {
                    Variables.Add(var);
                }
            }
        }
        protected List<Variable> FindReadVariables(Node n) {
            var collect = new VariableCollection();
            collect.Start(n);
            return collect.Variables;
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
