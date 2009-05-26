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

namespace PLR.AST.Expressions {
    public abstract class Expression : Node{
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }
        public abstract Type Type { get; }
    }
}
