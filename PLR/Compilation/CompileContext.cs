using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;
using PLR.AST;

namespace PLR.Compilation {
    public class CompileContext {

        private ModuleBuilder _module;
        private List<ILGenerator> _ilStack = new List<ILGenerator>();
        private List<TypeBuilder> _typeStack = new List<TypeBuilder>();
        private List<string> _importedClasses = new List<string>();
        private List<Assembly> _referencedAssemblies = new List<Assembly>();
        private Dictionary<string, ConstructorBuilder> _namedProcessConstructors = new Dictionary<string, ConstructorBuilder>();
        private CompileOptions _options;
        private ISymbolDocumentWriter _debugWriter;
        private Dictionary<string, Dictionary<string, FieldBuilder>> _typeFields = new Dictionary<string, Dictionary<string, FieldBuilder>>();
        private string _procdefName;
        public CompileOptions Options {
            get { return _options; }
            set { _options = value; }
        }
        
        public string ProcessName {
            get { return _procdefName; }
            set { _procdefName = value; }
        }

        public void StartNewProcessDefiniton(string procName) {
            ProcessName = procName;
            _typeFields.Add(procName, new Dictionary<string, FieldBuilder>());
        }

        public void AddField(string typeName, FieldBuilder field) {
            if (!_typeFields.ContainsKey(typeName)) {
                _typeFields.Add(typeName, new Dictionary<string, FieldBuilder>());
            }
            _typeFields[typeName].Add(field.Name, field);
        }

        public Dictionary<string, FieldBuilder> ProcessFields {
            get { return _typeFields[ProcessName]; }
        }

        public FieldBuilder GetField(string field) {
            if (_typeFields[this.Type.FullName].ContainsKey(field)) {
                return _typeFields[this.Type.FullName][field];
            }
            return null;
        }

        public FieldBuilder GetField(string typeName, string field) {
            if (_typeFields[typeName].ContainsKey(field)) {
                return _typeFields[typeName][field];
            }
            return null;
        }

        public Dictionary<string, FieldBuilder> GetFields() {
            return _typeFields[this.Type.FullName];
        }

        public Dictionary<string,FieldBuilder> GetFields(string typeName) {
            return _typeFields[typeName];
        }

        public ISymbolDocumentWriter DebugWriter {
            get { return _debugWriter; }
            set { _debugWriter = value; }
        }

        public ModuleBuilder Module {
            get { return _module; }
            set { _module = value; }
        }

        public Dictionary<string, ConstructorBuilder> NamedProcessConstructors {
            get {
                return _namedProcessConstructors;
            }
        }
        public List<String> ImportedClasses {
            get { return _importedClasses; }
            set { _importedClasses = value; }
        }

        public List<Assembly> ReferencedAssemblies {
            get { return _referencedAssemblies; }
        }

        public ILGenerator ILGenerator {
            get {
                if (_ilStack.Count == 0) {
                    return null;
                } else {
                    return _ilStack[_ilStack.Count - 1];
                }
            }
        }

        public TypeBuilder Type {
            get {
                if (_typeStack.Count == 0) {
                    return null;
                } else {
                    return _typeStack[_typeStack.Count - 1];
                }
            }
        }

        public void PushType(TypeBuilder type) {
            _typeStack.Add(type);
        }

        public void PushIL(ILGenerator il) {
            _ilStack.Add(il);
        }

        public TypeBuilder PopType() {
            TypeBuilder tb = this.Type;
            if (_typeStack.Count > 0) {
                _typeStack.RemoveAt(_typeStack.Count - 1);
            }
            return tb;
        }

        public ILGenerator PopIL() {
            ILGenerator il = this.ILGenerator;
            if (_ilStack.Count > 0) {
                _ilStack.RemoveAt(_ilStack.Count - 1);
            }
            return il;
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

        public void MarkSequencePoint(LexicalInfo lexinfo) {
            if (this.Options.Debug) {
                this.ILGenerator.MarkSequencePoint(DebugWriter, lexinfo.StartLine, lexinfo.StartColumn, lexinfo.EndLine, lexinfo.EndColumn);
            }
            

        }

        private Dictionary<TypeBuilder, bool> _restrictedTypes = new Dictionary<TypeBuilder, bool>();
        public void AddRestrictedType(TypeBuilder type) {
            if (!_restrictedTypes.ContainsKey(type)) {
                _restrictedTypes.Add(type, true);
            }
        }

        public bool IsRestricted(TypeBuilder type) {
            return _restrictedTypes.ContainsKey(type);
        }
    }
}
