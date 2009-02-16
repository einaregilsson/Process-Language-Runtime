namespace PLR.AST.Actions {

    public abstract class Action : Node{
        
        protected string _name;
        public string Name {
            get { return _name;}
        }
        
        public Action(string name) {
            _name = name;
        }
    }
}
