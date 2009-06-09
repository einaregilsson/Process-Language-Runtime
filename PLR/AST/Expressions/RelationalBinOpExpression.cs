/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Expressions {

    public enum RelationalBinOp {
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Equal,
        NotEqual
    }

    public class RelationalBinOpExpression : BooleanExpression {

        public RelationalBinOpExpression(Expression left, Expression right, RelationalBinOp op) {
            _op = op;
            _children.Add(left);
            _children.Add(right);
        }

        private RelationalBinOp _op;
        public Expression Left { get { return (Expression)_children[0]; } }
        public Expression Right { get { return (Expression)_children[1]; } }
        public RelationalBinOp Op { get { return _op; } }

        public override string ToString() {
            switch (this.Op) {
                case RelationalBinOp.Equal:
                    return Left + " == " + Right;
                case RelationalBinOp.NotEqual:
                    return Left + " != " + Right;
                case RelationalBinOp.GreaterThan:
                    return Left + " > " + Right;
                case RelationalBinOp.GreaterThanOrEqual:
                    return Left + " >= " + Right;
                case RelationalBinOp.LessThan:
                    return Left + " < " + Right;
                case RelationalBinOp.LessThanOrEqual:
                    return Left + " <= " + Right;
                default:
                    throw new Exception("Unknown relational binary operation: " + this.Op);
            }
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public override void Compile(CompileContext context) {
            ILGenerator il = context.ILGenerator;
            Left.Compile(context); if (Left is Variable) il.Emit(OpCodes.Unbox_Any, typeof(int));
            Right.Compile(context); if (Right is Variable) il.Emit(OpCodes.Unbox_Any, typeof(int));
            switch (this.Op) {
                case RelationalBinOp.Equal:
                    il.Emit(OpCodes.Ceq);
                    break;
                case RelationalBinOp.NotEqual:
                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                case RelationalBinOp.GreaterThan:
                    il.Emit(OpCodes.Cgt);
                    break;
                case RelationalBinOp.LessThan:
                    il.Emit(OpCodes.Clt);
                    break;
                case RelationalBinOp.GreaterThanOrEqual:
                    il.Emit(OpCodes.Clt);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                case RelationalBinOp.LessThanOrEqual:
                    il.Emit(OpCodes.Cgt);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                default:
                    throw new Exception("Unknown relational binary operation: " + this.Op);
            }

        }

    }
}
