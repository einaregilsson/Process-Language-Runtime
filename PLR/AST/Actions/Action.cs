namespace PLR.AST.Actions {

    public abstract class Action : Node{
        
        public string Name;// { get; private set; }
        
        public Action(string name) {
            Name = name;
        }
    }
}
