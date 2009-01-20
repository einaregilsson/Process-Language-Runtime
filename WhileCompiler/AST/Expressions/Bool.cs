using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Expressions {
    public class Bool : BooleanExpression{
        public Bool(bool value) {
            this.BoolValue = Value;
        }
        public override bool BoolValue { get; protected set;}
    }
}
