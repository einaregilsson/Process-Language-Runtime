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

    }
}
