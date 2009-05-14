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
        public override void Visit(ActionRestrictions restrictions) { }
        public override void Visit(ChannelRestrictions restrictions) { }
        public override void Visit(CustomPreprocess preprocess) { }
        public override void Visit(CustomRestrictions restrictinos) { }
        public override void Visit(PreProcessActions actions) { }
        public override void Visit(RelabelActions actions) { }

        //Actions
        public override void Visit(Action act) { }
        
        public override void Visit(Call call) { 
        }

        public override void Visit(InAction act) {
            CurrentList.Add(act.Name);
        }

        public override void Visit(OutAction act) {
            CurrentList.Add(act.Name);
        }

        //Expressions
        public override void Visit(ArithmeticBinOpExpression node) { }
        public override void Visit(ArithmeticExpression node) { }
        public override void Visit(Bool node) { }
        public override void Visit(BooleanExpression node) { }
        public override void Visit(Expression node) { }
        public override void Visit(LogicalBinOpExpression node) { }
        public override void Visit(MethodCallExpression node) { }
        public override void Visit(MethodInvokeBase node) { }
        public override void Visit(NewObject node) { }
        public override void Visit(Number node) { }
        public override void Visit(PLRString node) { }
        public override void Visit(RelationalBinOpExpression node) { }
        public override void Visit(ThisPointer node) { }
        public override void Visit(TypedNull node) { }
        public override void Visit(UnaryMinus node) { }
        
        public override void Visit(Variable node) { 
        
        }

        //Processes
        public override void Visit(ActionPrefix node) { }
        public override void Visit(BranchProcess node) { }
        public override void Visit(NilProcess node) { }
        public override void Visit(NonDeterministicChoice node) { }
        public override void Visit(ParallelComposition node) { }
        public override void Visit(Process node) { }
        public override void Visit(ProcessConstant node) { }

        //Root namespace nodes
        public override void Visit(ProcessSystem node) { }
        public override void Visit(ExpressionList node) { }
        public override void Visit(ProcessDefinition node) { }
        public override void Visit(Node node) { }
    }
}
