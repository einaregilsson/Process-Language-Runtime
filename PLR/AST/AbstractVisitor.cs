/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST;
using PLR.AST.Actions;
using PLR.AST.ActionHandling;
using PLR.AST.Expressions;
using PLR.AST.Processes;
using PLR.AST.Interfaces;

namespace PLR.AST
{
    public abstract class AbstractVisitor
    {
        public void Start(Node node) {
            VisitRecursive(node);
        }

        public bool VisitParentBeforeChildren { get; set; }
        protected virtual void VisitRecursive(Node node)
        {
            if (node == null) {
                return;
            }
            if (VisitParentBeforeChildren) {
                node.Accept(this);
            }
            foreach (Node child in node)
            {
                VisitRecursive(child);
            }
            if (!VisitParentBeforeChildren) {
                node.Accept(this);
            }
            
        }

        //ActionHandling
        public virtual void Visit(ActionRestrictions restrictions) { }
        public virtual void Visit(ChannelRestrictions restrictions) { }
        public virtual void Visit(CustomPreprocess preprocess) { }
        public virtual void Visit(CustomRestrictions restrictinos) { }
        public virtual void Visit(PreProcessActions actions) { }
        public virtual void Visit(RelabelActions actions) { }

        //Actions
        public virtual void Visit(Action act) { }
        public virtual void Visit(Call call) { }
        public virtual void Visit(InAction act) { }
        public virtual void Visit(OutAction act) { }

        //Expressions
        public virtual void Visit(ArithmeticBinOpExpression node) {}
        public virtual void Visit(ArithmeticExpression node) {}
        public virtual void Visit(Bool node) {}
        public virtual void Visit(BooleanExpression node) {}
        public virtual void Visit(Expression node) {}
        public virtual void Visit(LogicalBinOpExpression node) {}
        public virtual void Visit(MethodCallExpression node) {}
        public virtual void Visit(MethodInvokeBase node) {}
        public virtual void Visit(NewObject node) {}
        public virtual void Visit(Number node) {}
        public virtual void Visit(PLRString node) {}
        public virtual void Visit(RelationalBinOpExpression node) {}
        public virtual void Visit(ThisPointer node) {}
        public virtual void Visit(TypedNull node) {}
        public virtual void Visit(UnaryMinus node) {}
        public virtual void Visit(Variable node) { }

        //Processes
        public virtual void Visit(ActionPrefix node) {}
        public virtual void Visit(BranchProcess node) {}
        public virtual void Visit(NilProcess node) {}
        public virtual void Visit(NonDeterministicChoice node) {}
        public virtual void Visit(ParallelComposition node) {}
        public virtual void Visit(Process node) {}
        public virtual void Visit(ProcessConstant node) {}
        
        //Root namespace nodes
        public virtual void Visit(ProcessSystem node) { }
        public virtual void Visit(ExpressionList node) { }
        public virtual void Visit(ProcessDefinition node) { }
        public virtual void Visit(Node node) { }

        public virtual void Visit(IVariableAssignment assign) {}
        public virtual void Visit(IVariableReader reader) { }
    }
}
