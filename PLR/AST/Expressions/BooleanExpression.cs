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

namespace PLR.AST.Expressions {
    public abstract class BooleanExpression : Expression {
        public override Type Type {
            get { return typeof(bool); }
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

    }
}
