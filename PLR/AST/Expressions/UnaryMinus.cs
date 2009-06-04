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
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.Analysis.Expressions {

    public class UnaryMinus : ArithmeticExpression{
        protected Expression _exp;
        public Expression Expression { get { return _exp; } }

        public UnaryMinus(Expression exp) {
            _exp = exp;
            _children.Add(exp);
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }
        
        public override string ToString() {
            return "-" + _exp.ToString();
        }

        public override void Compile(CompileContext context) {
            _exp.Compile(context); if (_exp is Variable) context.ILGenerator.Emit(OpCodes.Unbox_Any, typeof(int));
            context.ILGenerator.Emit(OpCodes.Neg);
        }
    }
}
