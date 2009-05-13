using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST;
using PLR.AST.ActionHandling;
using PLR.AST.Actions;
using PLR.AST.Expressions;
using PLR.AST.Formatters;
using PLR.AST.Processes;


namespace PLR.AST.Formatters {
    public class BaseFormatter : AbstractVisitor{

        protected Stack<List<string>> _childStrings = new Stack<List<string>>();
        protected override void VisitRecursive(Node node) {
            _childStrings.Push(new List<string>());
            foreach (Node child in node) {
                VisitRecursive(child);
            }
            node.Accept(this);
        }

        protected List<string> CurrentList {
            get { return _childStrings.Peek(); }
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
        
        public virtual void Visit(Call call) { 
        }

        public virtual void Visit(InAction act) {
            CurrentList.Add(act.Name);
        }

        public virtual void Visit(OutAction act) {
            CurrentList.Add(act.Name);
        }

        //Expressions
        public virtual void Visit(ArithmeticBinOpExpression node) { }
        public virtual void Visit(ArithmeticExpression node) { }
        public virtual void Visit(Bool node) { }
        public virtual void Visit(BooleanExpression node) { }
        public virtual void Visit(Expression node) { }
        public virtual void Visit(LogicalBinOpExpression node) { }
        public virtual void Visit(MethodCallExpression node) { }
        public virtual void Visit(MethodInvokeBase node) { }
        public virtual void Visit(NewObject node) { }
        public virtual void Visit(Number node) { }
        public virtual void Visit(PLRString node) { }
        public virtual void Visit(RelationalBinOpExpression node) { }
        public virtual void Visit(ThisPointer node) { }
        public virtual void Visit(TypedNull node) { }
        public virtual void Visit(UnaryMinus node) { }
        
        public virtual void Visit(Variable node) { 
        
        }

        //Processes
        public virtual void Visit(ActionPrefix node) { }
        public virtual void Visit(BranchProcess node) { }
        public virtual void Visit(NilProcess node) { }
        public virtual void Visit(NonDeterministicChoice node) { }
        public virtual void Visit(ParallelComposition node) { }
        public virtual void Visit(Process node) { }
        public virtual void Visit(ProcessConstant node) { }

        //Root namespace nodes
        public virtual void Visit(ProcessSystem node) { }
        public virtual void Visit(ExpressionList node) { }
        public virtual void Visit(ProcessDefinition node) { }
        public virtual void Visit(Node node) { }
    }
}
