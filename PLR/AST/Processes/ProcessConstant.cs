using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using PLR.AST;
using PLR.AST.Expressions;
using PLR.Compilation;
using PLR.Runtime;

namespace PLR.AST.Processes
{
    public class ProcessConstant : Process {
        public ProcessConstant(string name) {
            _name = name;
            _expressions = new ExpressionList();
            _children.Add(Expressions);
        }

        private string _name;
        public string Name { get { return _name; } }

        protected ExpressionList _expressions;
        public ExpressionList Expressions { get { return _expressions; } }
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
            return other.Name == this.Name && other.Expressions.Count == this.Expressions.Count;
        }

        public override int GetHashCode() {
            return (this.Name + this.Expressions.Count).GetHashCode();

        }
        public string FullName {
            get {
                if (this.Expressions.Count > 0) {
                    return this.Name + "_" + this.Expressions.Count;
                }
                return this.Name;
            }
        }

        public override void Compile(CompileContext context) {

            foreach (ArithmeticExpression exp in this.Expressions) {
                exp.Compile(context);
            }
            EmitRunProcess(context, context.GetType(this.FullName).Constructor, false, LexicalInfo);
        }
    }
}
