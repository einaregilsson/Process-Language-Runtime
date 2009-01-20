using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Expressions {
    public abstract class BooleanExpression : Expression{
        public abstract bool BoolValue { get; protected set; }
        public override object Value {
            get { return this.BoolValue; }
        }
    }
}
