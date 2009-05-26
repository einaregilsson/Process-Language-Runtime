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
using System.Text;
using PLR.AST.Expressions;
using PLR.Compilation;

namespace PLR.AST {
    public class ExpressionList : Node {

        public void Add(Expression exp) {
            _children.Add(exp);
        }

        public new Expression this[int index] {
            get { return (Expression)_children[index]; }
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
        }
    }
}
