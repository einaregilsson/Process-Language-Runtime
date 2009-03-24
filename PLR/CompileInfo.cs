using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;

namespace PLR {
    public class CompileInfo {

        private ModuleBuilder _module;
        private ILGenerator _il;
        private TypeBuilder _type;
        private MethodBuilder _preprocess;
        private MethodBuilder _restrict;

        public ModuleBuilder Module {
            get { return _module; }
            set { _module = value; }
        }

        public ILGenerator ILGenerator {
            get { return _il; }
            set { _il = value; }
        }

        public TypeBuilder Type {
            get { return _type; }
            set { _type = value; }
        }

        public MethodBuilder PreProcess {
            get { return _preprocess; }
            set { _preprocess = value; }
        }

        public MethodBuilder Restrict {
            get { return _restrict; }
            set { _restrict = value; }
        }
    }
}
