using System;
using System.Collections.Generic;
using System.Text;
using PLR.Compilation;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {
    public enum LogicalBinOp {
        And,
        Or,
        Xor
    }

    public class LogicalBinOpExpression : BooleanExpression {

        public LogicalBinOpExpression(Expression left, Expression right, LogicalBinOp op) {
            _op = op;
            _children.Add(left);
            _children.Add(right);
        }

        private LogicalBinOp _op;
        public Expression Left { get { return (Expression)_children[0]; } }
        public Expression Right { get { return (Expression)_children[1]; } }
        public LogicalBinOp Op { get { return _op; } }

        public override string ToString() {
            switch (this.Op) {
                case LogicalBinOp.And:
                    return Left + " and " + Right;
                case LogicalBinOp.Or:
                    return Left + " or" + Right;
                case LogicalBinOp.Xor:
                    return Left + " xor " + Right;
                default:
                    throw new Exception("Unknown logical binary operation: " + this.Op);
            }
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            ILGenerator il = context.ILGenerator;
            Left.Compile(context);
            Right.Compile(context);
            switch (this.Op) {
                case LogicalBinOp.And:
                    il.Emit(OpCodes.And);
                    break;
                case LogicalBinOp.Or:
                    il.Emit(OpCodes.Or);
                    break;
                case LogicalBinOp.Xor:
                    il.Emit(OpCodes.Xor);
                    break;
                default:
                    throw new Exception("Unknown logical binary operation: " + this.Op);
            }
        }
    }
}
