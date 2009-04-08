using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Expressions {

    public enum ArithmeticBinOp {
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo
    }

    public class ArithmeticBinOpExpression : ArithmeticExpression {

        public ArithmeticBinOpExpression(Expression left, Expression right, ArithmeticBinOp op) {
            _op = op;
            _children.Add(left);
            _children.Add(right);
        }

        private ArithmeticBinOp _op;

        public Expression Left { get { return (Expression)_children[0]; } }
        public Expression Right { get { return (Expression)_children[1]; } }
        public ArithmeticBinOp Op { get { return _op; } }

        public override string ToString() {
            switch (this.Op) {
                case ArithmeticBinOp.Divide:
                    return Left + "/" + Right;
                case ArithmeticBinOp.Minus:
                    return Left + "-" + Right;
                case ArithmeticBinOp.Multiply:
                    return Left + "*" + Right;
                case ArithmeticBinOp.Plus:
                    return Left + "+" + Right;
                case ArithmeticBinOp.Modulo:
                    return Left + "%" + Right;
                default:
                    throw new Exception("Unknown arithmetic binary operation: " + this.Op);
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
                case ArithmeticBinOp.Divide:
                    il.Emit(OpCodes.Div);
                    break;
                case ArithmeticBinOp.Minus:
                    il.Emit(OpCodes.Sub_Ovf);
                    break;
                case ArithmeticBinOp.Multiply:
                    il.Emit(OpCodes.Mul_Ovf);
                    break;
                case ArithmeticBinOp.Plus:
                    il.Emit(OpCodes.Add_Ovf);
                    break;
                case ArithmeticBinOp.Modulo:
                    il.Emit(OpCodes.Rem);
                    break;
                default:
                    throw new Exception("Unknown arithmetic binary operation: " + this.Op);
            }

        }

    }
}
