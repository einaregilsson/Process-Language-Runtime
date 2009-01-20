using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Expressions {
    public class ArithmeticBinaryOp : ArithmeticExpression{

        public const string Plus = "+";
        public const string Minus  = "-";
        public const string Multiplication = "*";
        public const string Division = "/";
        public const string Modulo = "%";
        public const string BitAnd = "&";
        public const string BitOr = "|";
        public const string BitXor = "^";
        public const string BitShiftLeft = "<<";
        public const string BitShiftRight = ">>";

        public ArithmeticExpression Left { get; private set; }
        public ArithmeticExpression Right { get; private set; }
        public string Op { get; private set; }

        public ArithmeticBinaryOp(string op, ArithmeticExpression left, ArithmeticExpression right) {
            this.Left = left;
            this.Right = right;
            this.Op = op;
        }

        public override List<Node> GetChildren() {
            return new List<Node>() { this.Left, this.Right };
        }

        public override int IntValue {
            get {
                switch (this.Op) {
                    case Plus: return this.Left.IntValue + this.Right.IntValue;
                    case Minus: return this.Left.IntValue - this.Right.IntValue;
                    case Multiplication: return this.Left.IntValue * this.Right.IntValue;
                    case Division: return this.Left.IntValue / this.Right.IntValue;
                    case Modulo: return this.Left.IntValue % this.Right.IntValue;
                    case BitAnd: return this.Left.IntValue & this.Right.IntValue;
                    case BitOr: return this.Left.IntValue | this.Right.IntValue;
                    case BitXor: return this.Left.IntValue ^ this.Right.IntValue;
                    case BitShiftLeft: return this.Left.IntValue << this.Right.IntValue;
                    case BitShiftRight: return this.Left.IntValue >> this.Right.IntValue;
                    default: throw new Exception("Unrecognized op: " + this.Op);
                }
            }
            protected set {
                //Do nothing
            }
        }
    }
}
