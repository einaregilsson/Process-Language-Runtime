
namespace PLR.AST.Actions {
    public class InAction : Action {
        public InAction(string name) : base(name) {}
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

    }
}
