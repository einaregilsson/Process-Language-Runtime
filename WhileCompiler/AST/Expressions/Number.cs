using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace While.AST.Expressions {
    public class Number : Expression{
        private int nr;
        public Number(int nr) {
            this.nr = nr;
        }
    }
}
