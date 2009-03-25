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
        public void VisitProcessTree(ProcessSystem system) {
            VisitRecursive(system);
        }

        protected void VisitRecursive(Node node)
        {
            foreach (Node child in node)
            {
                VisitRecursive(child);
            }
            node.Accept(this);
        }

        public abstract void Visit(ProcessSystem system);
        public abstract void Visit(ActionID id);
        public abstract void Visit(InAction act);
        public abstract void Visit(OutAction act);
        public abstract void Visit(ArithmeticBinOpExpression exp);
        public abstract void Visit(Number exp);
        public abstract void Visit(UnaryMinus exp);
        public abstract void Visit(Variable var);
        public abstract void Visit(ActionPrefix proc);
        public abstract void Visit(NilProcess proc);
        public abstract void Visit(NonDeterministicChoice proc);
        public abstract void Visit(ParallelComposition proc);
        public abstract void Visit(ProcessConstant proc);
        public abstract void Visit(ProcessDefinition procdef);
        public abstract void Visit(PreProcessActions relabellings);
        public abstract void Visit(RelabelActions actions);
        public abstract void Visit(ActionRestrictions res);
        public abstract void Visit(ChannelRestrictions res);
        public abstract void Visit(Subscript subscript);
        public abstract void Visit(Node node);
    }
}
