using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST.Expressions {

    public enum ArithmeticBinOp {
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo
    }

    public class ArithmeticBinOpExpression : ArithmeticExpression{

        public ArithmeticBinOpExpression(ArithmeticExpression left, ArithmeticExpression right, ArithmeticBinOp op) {
            _right = right;
            _left = left;
            _op = op;
            _children.Add(left);
            _children.Add(right);
        }

        private ArithmeticExpression _right;
        private ArithmeticExpression _left;
        private ArithmeticBinOp _op;

        public ArithmeticExpression Right { get {return _right;}}
        public ArithmeticExpression Left { get {return _left;}}
        public ArithmeticBinOp Op { get {return _op;}}

        public override int Value {
            get {
                switch (this.Op) {
                    case ArithmeticBinOp.Divide:
                        return Left.Value / Right.Value;
                    case ArithmeticBinOp.Minus:
                        return Left.Value - Right.Value;
                    case ArithmeticBinOp.Multiply:
                        return Left.Value * Right.Value;
                    case ArithmeticBinOp.Plus:
                        return Left.Value + Right.Value;
                    case ArithmeticBinOp.Modulo:
                        return Left.Value % Right.Value;
                    default:
                        throw new Exception("Unknown arithmetic binary operation: " + this.Op);
                }
            }
        }

        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override void Compile(ILGenerator il) {
            _left.Compile(il);
            _right.Compile(il);
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
