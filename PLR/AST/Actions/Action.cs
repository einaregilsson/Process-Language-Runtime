using System.Reflection;
using PLR.Runtime;

namespace PLR.AST.Actions {

    public abstract class Action : Node{

        protected string _name;
        public string Name {
            get { return _name;}
        }
        public Action(string name) {
            _name = name;
        }

        protected MethodInfo SyncMethod {
            get {
                return typeof(ProcessBase).GetMethod("Sync", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
    }
}
