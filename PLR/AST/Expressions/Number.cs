/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.Analysis.Expressions {

    public class Number : ArithmeticExpression {
        private int _number;
        public Number(int number) {
            _number = number;
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public void Print(string s) { }
        public void junk() {
            Print("Printing string");
            Print("Print expr: " + (3 + 2 / 4));
        }
        public int Value {
            get { return _number; }
        }
        public override string ToString() {
            return _number.ToString();
        }

        public override void Compile(CompileContext context) {
            context.ILGenerator.Emit(OpCodes.Ldc_I4, _number);
        }
    }
}
