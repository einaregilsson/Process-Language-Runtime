using System.Text.RegularExpressions;

namespace PLR.AST.Actions
{
    public class OutAction : Action
    {
        public OutAction(string name) : base(Regex.Replace(name, "^_|_$","")) { }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

    }
}
