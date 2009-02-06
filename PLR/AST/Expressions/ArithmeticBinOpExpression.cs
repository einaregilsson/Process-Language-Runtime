using System;
using System.Collections.Generic;

using System.Text;

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
            this.Right = right;
            this.Left = left;
            this.Op = op;
            _children.Add(left);
            _children.Add(right);
        }

        public ArithmeticExpression Right;// { get; private set; }
        public ArithmeticExpression Left;// { get; private set; }
        public ArithmeticBinOp Op;// { get; private set; }

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
    }
}
