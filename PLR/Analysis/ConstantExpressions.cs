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
using PLR.Analysis.Expressions;
using PLR.Analysis.Processes;

namespace PLR.Analysis {
    
    public class FoldConstantExpressions : AbstractVisitor, IAnalysis{

        public FoldConstantExpressions() {
            base.VisitParentBeforeChildren = false;
        }

        public List<Warning> Analyze(ProcessSystem system) {
            this.Start(system);
            return new List<Warning>();
        }

        public override void Visit(LogicalBinOpExpression exp) {
            if (exp.Left is Bool && exp.Right is Bool) {
                bool result = false, leftVal, rightVal;
                leftVal = ((Bool)exp.Left).Value;
                rightVal = ((Bool)exp.Right).Value;

                if (exp.Op == LogicalBinOp.And) {
                    result = leftVal && rightVal;
                } else if (exp.Op == LogicalBinOp.Or) {
                    result = leftVal || rightVal;
                } else if (exp.Op == LogicalBinOp.Xor) {
                    result = leftVal ^ rightVal;
                }
                exp.Parent.Replace(exp, new Bool(result));
            }
        }

        public override void Visit(BranchProcess p) {
            
            if (p.Expression is Bool) { //Has been optimized completely away...
                bool value = ((Bool)p.Expression).Value;
                if (value) {
                    p.ElseBranch.IsUsed = false;
                } else {
                    p.IfBranch.IsUsed = false;
                }
            }
        }
        public override void Visit(RelationalBinOpExpression exp) {
            if (exp.Left is Number && exp.Right is Number) {
                bool result = false;
                int leftVal, rightVal;
                leftVal = ((Number)exp.Left).Value;
                rightVal = ((Number)exp.Right).Value;

                if (exp.Op == RelationalBinOp.Equal) {
                    result = rightVal == leftVal;
                } else if (exp.Op == RelationalBinOp.GreaterThan) {
                    result = leftVal > rightVal;
                } else if (exp.Op == RelationalBinOp.GreaterThanOrEqual) {
                    result = leftVal >= rightVal;
                } else if (exp.Op == RelationalBinOp.LessThan) {
                    result = leftVal < rightVal;
                } else if (exp.Op == RelationalBinOp.LessThanOrEqual) {
                    result = leftVal <= rightVal;
                } else if (exp.Op == RelationalBinOp.NotEqual) {
                    result = leftVal != rightVal;
                }
                exp.Parent.Replace(exp, new Bool(result));
            }
        }

        public override void Visit(ArithmeticBinOpExpression exp) {
            if (exp.Left is Number && exp.Right is Number) {
                int result = 0, leftVal, rightVal;
                leftVal = ((Number)exp.Left).Value;
                rightVal = ((Number)exp.Right).Value;

                if (exp.Op == ArithmeticBinOp.Plus) {
                    result = rightVal + leftVal;
                } else if (exp.Op == ArithmeticBinOp.Minus) {
                    result = leftVal - rightVal;
                } else if (exp.Op == ArithmeticBinOp.Multiply) {
                    result = leftVal * rightVal;
                } else if (exp.Op == ArithmeticBinOp.Divide) {
                    result = leftVal / rightVal;
                }
                exp.Parent.Replace(exp, new Number(result));
            }
        }

        public override void Visit(UnaryMinus exp) {
            if (exp.Expression is Number) {
                exp.Parent.Replace(exp, new Number(-((Number)exp.Expression).Value));
            }
        }
    }
}
