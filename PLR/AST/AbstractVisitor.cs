using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST;
using PLR.AST.Actions;
using PLR.AST.ActionHandling;
using PLR.AST.Expressions;
using PLR.AST.Processes;

namespace PLR.AST
{
    public abstract class AbstractVisitor
    {
        public void Start(Node node) {
            VisitRecursive(node);
        }

        protected void VisitRecursive(Node node)
        {
            foreach (Node child in node)
            {
                VisitRecursive(child);
            }
            node.Accept(this);
        }

        public virtual void Visit(ProcessSystem system){}
        public virtual void Visit(InAction act){}
        public virtual void Visit(OutAction act){}
        public virtual void Visit(ArithmeticBinOpExpression exp) { }
        public virtual void Visit(Number exp) { }
        public virtual void Visit(UnaryMinus exp) { }
        public virtual void Visit(Variable var) { }
        public virtual void Visit(ActionPrefix proc) { }
        public virtual void Visit(NilProcess proc) { }
        public virtual void Visit(NonDeterministicChoice proc) { }
        public virtual void Visit(ParallelComposition proc) { }
        public virtual void Visit(ProcessConstant proc) { }
        public virtual void Visit(ProcessDefinition procdef) { }
        public virtual void Visit(PreProcessActions relabellings) { }
        public virtual void Visit(RelabelActions actions) { }
        public virtual void Visit(ActionRestrictions res) { }
        public virtual void Visit(ChannelRestrictions res) { }
        public virtual void Visit(ExpressionList expressions) { }
        public virtual void Visit(Node node) { }
    }
}
