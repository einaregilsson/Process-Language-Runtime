using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PLR.AST.Expressions;

namespace PLR.AST {

    public abstract class Node : IEnumerable<Node> {

        public abstract void Accept(AbstractVisitor visitor);
        //Source file information

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

        public abstract void Compile(CompileInfo info);

        protected void Assign(LocalBuilder local, Expression exp, CompileInfo info) {
            exp.Compile(info);
            info.ILGenerator.Emit(OpCodes.Stloc, local);
        }

        protected MethodCall Call(LocalBuilder instance, string methodName, bool popReturn, params object[] args) {
            MethodCall mc = new MethodCall(instance, methodName, args);
            mc.PopReturnValue = popReturn;
            return mc;
        }
        
        protected MethodCall Call(Expression instance, string methodName, bool popReturn, params object[] args) {
            MethodCall mc = new MethodCall(instance, methodName, args);
            mc.PopReturnValue = popReturn;
            return mc;
        }

        protected MethodCall Call(Type type, string methodName, bool popReturn, params object[] args) {
            MethodCall mc = new MethodCall(type, methodName, args);
            mc.PopReturnValue = popReturn;
            return mc;
        }


        public NewObject New(Type type, params object[] args) {
            return new NewObject(type, args);
        }

        protected void EmitDebug(string msg, CompileInfo info) {
            ThisPointer thisP = new ThisPointer(typeof(ProcessBase));
            Call(thisP, "Debug", true, msg).Compile(info);
        }
        protected void CallScheduler(string methodName, bool popReturn, CompileInfo info, params object[] args) {
            Call(Call(typeof(Scheduler), "get_Instance", false), methodName, popReturn, args).Compile(info);
        }
            

        #endregion


    }
}
