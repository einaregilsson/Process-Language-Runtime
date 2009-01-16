using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Statements {
    public class Block : Statement{
        public Block(IEnumerable<VariableDeclaration> vars, StatementSequence stmtSequence){}
    }
}
