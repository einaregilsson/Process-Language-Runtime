
namespace PLR.AST.Actions {
    public class TauAction : Action{
        public TauAction() : base("t") {} 
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
