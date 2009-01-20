using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using While.AST;

namespace While.AST.Expressions {
    public abstract class Expression : Node {
        public abstract object Value { get; }
    }
}
