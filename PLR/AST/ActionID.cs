using System;
using System.Collections.Generic;
using System.Text;

namespace PLR.AST
{
    public class ActionID : Node
    {
        public string Name;
        public ActionID(string name)
        {
            this.Name = name;
        }
    }
}
