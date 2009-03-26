using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR {
    public class CompileContext {

        private ModuleBuilder _module;
        private ILGenerator _il;
        private TypeBuilder _type;
        private MethodBuilder _preprocess;
        private MethodBuilder _restrict;
        private List<string> _importedClasses = new List<string>();
        private List<Assembly> _referencedAssemblies = new List<Assembly>();

        public ModuleBuilder Module {
            get { return _module; }
            set { _module = value; }
        }

        public List<String> ImportedClasses {
            get { return _importedClasses; }
            set { _importedClasses = value; }
        }

        public List<Assembly> ReferencedAssemblies {
            get { return _referencedAssemblies; }
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

        public MethodInfo GetMethod(string methodName) {
            return GetMethod(methodName, null);
        }

        public MethodInfo GetMethod(string methodName, Type[] paramTypes) {
            MethodInfo method;
            foreach (Assembly assembly in this.ReferencedAssemblies) {
                foreach (string clazz in this.ImportedClasses) {
                    Type type = assembly.GetType(clazz);
                    if (type != null) {
                        if (paramTypes == null) {
                            method = type.GetMethod(methodName);
                        } else {
                            method = type.GetMethod(methodName, paramTypes);
                        }
                        
                        if (method != null) {
                            return method;
                        }
                    }
                }
            }
            return null;
        }
    }
}
