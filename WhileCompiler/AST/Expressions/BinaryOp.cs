using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Expressions {

    public class BinaryOp : Expression{
        public const string Plus = "+";
        public const string Minus  = "-";
        public const string Multiplication = "*";
        public const string Division = "/";
        public const string Modulo = "%";
        public const string GreaterThan = ">";
        public const string LessThan = "<";
        public const string GreaterThanOrEqual = ">=";
        public const string LessThanOrEqual = "<=";
        public const string Equal = "==";
        public const string NotEquals = "!=";
        public const string BitAnd = "&";
        public const string BitOr = "|";
        public const string BitXor = "^";
        public const string BitShiftLeft = "<<";
        public const string BitShiftRight = ">>";
        public const string LogicAnd = "&&";
        public const string LogicOr = "||";

        
        public BinaryOp(string op, Expression left, Expression right) {
        }
    }
}
