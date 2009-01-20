using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Expressions {
    public class Variable : Expression{
        private string name;
        public Variable(string name) {
            this.name = name;
        }
    }
}
