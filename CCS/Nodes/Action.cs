namespace CCS.Nodes {

    public abstract class Action : ASTNode{
        
        public string Name { get; private set; }
        
        public Action(string name) {
            Name = name;
        }
    }
}
