using System;
using System.Collections.Generic;
using System.Text;

namespace PLR.AST
{
    public class ActionID : Node
    {
        protected string _name;
        public string Name { get { return _name; } }
        public ActionID(string name)
        {
            _name = name;
        }
    }
}
