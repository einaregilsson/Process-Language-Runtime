using System.Collections.Generic;
using PLR.AST;

namespace PLR.AST.Processes
{
    public class ProcessConstant : Process {
        public ProcessConstant() {
            Subscript = new Subscript();
            _children.Add(Subscript);
        }
        public string Name;// { get; set; }
        public Subscript Subscript;// { get; private set; }
    }
}
