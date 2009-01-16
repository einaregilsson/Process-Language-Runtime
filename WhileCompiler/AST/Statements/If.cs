using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using While.AST.Expressions;
using While.AST.Statements;

namespace While.AST.Statements {
    public class If : Statement {
        public If(Expression exp, StatementSequence ifBranch, StatementSequence elseBranch) { }
    }

}
