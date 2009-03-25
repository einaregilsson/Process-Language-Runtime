using System;
using System.Collections.Generic;
using System.Text;

namespace PLR.AST {
    public class ActionID : Node {
        protected string _name;
        public string Name { get { return _name; } }
        public ActionID(string name) {
            _name = name;
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileInfo info) {
        }
    }
}
