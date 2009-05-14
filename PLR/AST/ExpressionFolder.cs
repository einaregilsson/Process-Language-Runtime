using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST.Expressions;

namespace PLR.AST {
    class ExpressionFolder : AbstractVisitor{
        public override void Visit(ArithmeticBinOpExpression exp) {
            if (exp.Left is Number && exp.Right is Number) {
                int result = 0, leftVal, rightVal;
                leftVal = ((Number)exp.Left).Value;
                rightVal = ((Number)exp.Right).Value;

                if (exp.Op == ArithmeticBinOp.Plus) {
                    result = rightVal + leftVal;
                } else if (exp.Op == ArithmeticBinOp.Minus) {
                    result = rightVal + leftVal;
                } else if (exp.Op == ArithmeticBinOp.Multiply) {
                    result = rightVal * leftVal;
                } else if (exp.Op == ArithmeticBinOp.Divide) {
                    result = rightVal / leftVal;
                }
                int pos = exp.Parent.ChildNodes.IndexOf(exp);
                exp.Parent.ChildNodes[pos] = new Number(result);
            }
        }
    }
}
