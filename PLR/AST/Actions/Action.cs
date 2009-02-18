namespace PLR.AST.Actions {

    public abstract class Action : Node{

        private static int _idGenerator = 1;
        protected string _name;
        protected int _id;
        public string Name {
            get { return _name;}
        }
        public int ID {
            get { return _id;}
        }
        
        public Action(string name) {
            _name = name;
            _id = _idGenerator++;
        }
    }
}
