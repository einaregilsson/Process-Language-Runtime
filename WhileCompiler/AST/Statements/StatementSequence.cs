using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using While.AST;

namespace While.AST.Statements {

    public class StatementSequence : Node{
        public StatementSequence(IEnumerable<Statement> statements) { }
    }
}
