using System.Text.RegularExpressions;

namespace PLR.AST.Actions
{
    public class OutAction : Action
    {
        public OutAction(string name) : base(Regex.Replace(name, "^_|_$","")) { }
    }
}
