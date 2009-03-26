using PLR.AST;
using PLR.AST.Actions;
using PLR.AST.ActionHandling;
using System.Collections.Generic;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Processes {
    public abstract class Process : Node {
        private PreProcessActions _preprocess = null;
        public PreProcessActions PreProcessActions {
            get { return _preprocess; }
            set { _preprocess = value; }
        }
        
        private ActionRestrictions _restrictions = null;
        public ActionRestrictions ActionRestrictions { 
            get { return _restrictions; }
            set { _restrictions = value; }
        }

        public override void Compile(CompileContext context) {
            if (this.PreProcessActions != null) {
                this.PreProcessActions.Compile(context);
            }
            if (this.ActionRestrictions != null) {
                this.ActionRestrictions.Compile(context);
            }

        }

    }
}
