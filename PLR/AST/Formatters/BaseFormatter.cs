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
using System.Text;
using PLR.AST.ActionHandling;
using PLR.AST.Actions;
using PLR.AST.Expressions;
using PLR.AST.Processes;

namespace PLR.AST.Formatters {

    public class BaseFormatter : AbstractVisitor{

        public BaseFormatter() {
            _childStrings.Push(new List<string>());
        }
        protected Stack<List<string>> _childStrings = new Stack<List<string>>();
        protected override void VisitRecursive(Node node) {
            if (node == null) {
                Return("");
                return;
            }
            _childStrings.Push(new List<string>());
            foreach (Node child in node) {
                VisitRecursive(child);
            }
            node.Accept(this);
        }

        private string _returnValue;

        public string GetFormatted() {
            return _returnValue;
        }

        protected List<string> CurrentList {
            get { return _childStrings.Peek(); }
        }

        protected List<string> PopChildren() {
            return _childStrings.Pop();
        }


        /// <summary>
        /// Checks whether a process has action restrictions or
        /// relabellings, if so adds them to the string representation
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="children"></param>
        /// <returns></returns>
        protected string CheckProc(string proc, List<string> children) {
            if (children[0] != "" ||  children[1] != "") {
                proc = "(" + proc + ")" + children[0] + children[1]; 
            }
            return proc;
        }
        //ActionHandling
        
        public override void Visit(ChannelRestrictions restrictions) {
            PopChildren();
            if (restrictions.ChannelNames.Count == 0) {
                Return("");
            } else {
                Return(" \\ {" + Join(", ", restrictions.ChannelNames) + "}");
            }
        }

        public override void Visit(CustomRestrictions restrictions) {
            PopChildren();
            Return("\\ :" + restrictions.MethodName);
        }

        public override void Visit(CustomPreprocess preprocess) {
            PopChildren();
            Return("[:" + preprocess.MethodName + "]");
        }

        
        public override void Visit(RelabelActions actions) {
            PopChildren();
            StringBuilder sb = new StringBuilder();
            foreach (string key in actions.Mapping.Keys) {
                sb.Append(actions.Mapping[key] + "/" + key + ", ");
            }
            sb.Remove(sb.Length-2, 2);
            Return("[" + sb.ToString() + "]");
        }

        //Actions
        public override void Visit(Call call) {
            Return(PopChildren()[0]);
        }

        public override void Visit(InAction act) {
            List<string> children = PopChildren();
            if (children.Count == 0) {
                Return(act.Name);
            } else {
                Return(act.Name + "(" + Join(", ", children) + ")");
            }
        }

        public override void Visit(OutAction act) {
            List<string> children = PopChildren();
            if (children.Count == 0) {
                Return("_" + act.Name + "_");
            } else {
                Return("_" + act.Name + "_(" + Join(", ", children) + ")");
            }
        }

        protected void Return(string value) {
            CurrentList.Add(value);
            _returnValue = value;
        }

        //Expressions
        public override void Visit(ArithmeticBinOpExpression node) { 
            string op = "";
            if (node.Op == ArithmeticBinOp.Divide) {
                op = "/";
            } else if (node.Op == ArithmeticBinOp.Minus) {
                op = "-";
            } else if (node.Op == ArithmeticBinOp.Plus) {
                op = "+";
            } else if (node.Op == ArithmeticBinOp.Modulo) {
                op = "%";
            } else if (node.Op == ArithmeticBinOp.Multiply) {
                op = "*";
            }
            List<string> children = PopChildren();
            Return(SurroundWithParens(children[0] + op + children[1], node.ParenCount));
        }
        
        public override void Visit(Bool node) {
            PopChildren();
            Return(node.Value ? "true" : "false");
        }
        
        public override void Visit(LogicalBinOpExpression node) {
            string op = "";
            if (node.Op == LogicalBinOp.And) {
                op = "and";
            } else if (node.Op == LogicalBinOp.Or) {
                op = "or";
            } else if (node.Op == LogicalBinOp.Xor) {
                op = "xor";
            }
            List<string> children = PopChildren();
            Return(SurroundWithParens(children[0] +  op + children[1], node.ParenCount));
        }

        public override void Visit(RelationalBinOpExpression node) {
            string op = "";
            if (node.Op == RelationalBinOp.Equal) {
                op = "=";
            } else if (node.Op == RelationalBinOp.GreaterThan) {
                op = ">";
            } else if (node.Op == RelationalBinOp.GreaterThanOrEqual) {
                op = ">=";
            } else if (node.Op == RelationalBinOp.LessThan) {
                op = "<";
            } else if (node.Op == RelationalBinOp.LessThanOrEqual) {
                op = "<=";
            } else if (node.Op == RelationalBinOp.NotEqual) {
                op = "!=";
            }
            List<string> children = PopChildren();
            Return(SurroundWithParens(children[0] +  op + children[1], node.ParenCount));
        }

        public override void Visit(UnaryMinus node) {
            List<string> children = PopChildren();
            Return(SurroundWithParens("-" + children[0], node.ParenCount));        
        }
        
        public override void Visit(MethodCallExpression node) {
            List<string> children = PopChildren();
            Return(":" + node.MethodName + "(" + Join(", ", children) + ")");

        }
        
        public override void Visit(Number node) {
            PopChildren();
            Return(SurroundWithParens(node.Value.ToString(), node.ParenCount));
        }

        public override void Visit(PLRString node) {
            PopChildren();
            if (PLRString.DisplayWithoutQuotes) {
                Return(node.Value);
            } else {
                Return("\"" + node.Value + "\"");
            }
        }
        
        public override void Visit(Variable node) {
            PopChildren();
            Return(SurroundWithParens(node.Name, node.ParenCount));        
        }

        //Processes
        public override void Visit(ActionPrefix node) {
            List<string> children = PopChildren();
            Return(CheckProc(children[2] + " . " + children[3], children));
        }

        public override void Visit(BranchProcess node) {
            List<string> children = PopChildren();
            Return("if " + children[2] + " then " + children[3] + " else " + children[4]);
        }

        public override void Visit(NilProcess node) {
            List<string> children = PopChildren();
            Return("0" + children[0] + children[1]);
        }

        public override void Visit(NonDeterministicChoice node) {
            List<string> children = PopChildren();
            List<string> procChildren = new List<string>(children);
            procChildren.RemoveRange(0,2);
            Return(CheckProc(Join(" + ", procChildren), children));
        }

        protected string Join(string sep, List<string> items) {
            return string.Join(sep, items.ToArray());
        }

        public override void Visit(ParallelComposition node) {
            List<string> children = PopChildren();
            List<string> procChildren = new List<string>(children);
            procChildren.RemoveRange(0, 2);
            Return(CheckProc(Join(" | ", procChildren), children));
        }

        public override void Visit(ProcessConstant node) {
            List<string> children = PopChildren();
            Return(node.Name + children[2] + children[0] + children[1]);
        }

        //Root namespace nodes
        public override void Visit(ProcessSystem node) { 
            Return(Join("\n", PopChildren()));
        }

        public override void Visit(ExpressionList node) {
            List<string> children = PopChildren();
            if (children.Count == 0) {
                Return("");
            } else {
                Return("(" + Join(", ", children) + ")");
            }
        }

        protected static string SurroundWithParens(string s, int count) {
            return new String('(', count) + s + new String(')', count);
        }

        public override void Visit(ProcessDefinition node) {
            List<string> children = PopChildren();
            Return(node.Name + children[0] + " = " + children[1]);
        }
    }
}
