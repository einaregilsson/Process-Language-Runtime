/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
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
        private List<TypeInfo> _typeStack = new List<TypeInfo>();
        private List<string> _importedClasses = new List<string>();
        private List<Assembly> _referencedAssemblies = new List<Assembly>();
        private CompileOptions _options;
        private ISymbolDocumentWriter _debugWriter;
        private Dictionary<string, TypeInfo> _types = new Dictionary<string, TypeInfo>();
        private TypeInfo _currentMasterType;

        public void AddType(TypeInfo type) {
            _types.Add(type.Name, type);
        }

        public CompileOptions Options {
            get { return _options; }
            set { _options = value; }
        }
        
        public TypeInfo CurrentMasterType {
            get { return _currentMasterType; }
            set { _currentMasterType = value; }
        }

        public ISymbolDocumentWriter DebugWriter {
            get { return _debugWriter; }
            set { _debugWriter = value; }
        }

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
            get {
                if (_ilStack.Count == 0) {
                    return null;
                } else {
                    return _ilStack[_ilStack.Count - 1];
                }
            }
        }

        public TypeInfo Type {
            get {
                if (_typeStack.Count == 0) {
                    return null;
                } else {
                    return _typeStack[_typeStack.Count - 1];
                }
            }
        }

        public void PushType(TypeInfo type) {
            _typeStack.Add(type);
        }

        public void PushIL(ILGenerator il) {
            _ilStack.Add(il);
        }

        public TypeInfo PopType() {
            TypeInfo tb = this.Type;
            if (_typeStack.Count > 0) {
                _typeStack.RemoveAt(_typeStack.Count - 1);
            }
            return tb;
        }

        public TypeInfo GetType(string name) {
            int paramNr = 0;
            if (name.Contains("_")) {
                paramNr = int.Parse(name.Split('_')[1]);
            }
            if (!_types.ContainsKey(name)) {
                throw new Exception("Process " + name + " does not exist, at least not with " + paramNr + " parameters!");
            }
            return _types[name];
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
            MethodInfo method = null;
            foreach (Assembly assembly in this.ReferencedAssemblies) {
                foreach (string clazz in this.ImportedClasses) {
                    Type type = assembly.GetType(clazz);
                    if (type != null) {
                        try {
                            //Simplest case, if there is only one then lets use that!
                            method = type.GetMethod(methodName);
                            if (method != null) {
                                return method;
                            }
                        } catch (Exception) {
                            method = type.GetMethod(methodName, paramTypes);
                        }

                        if (method != null) {
                            return method;
                        }
                    }
                }
            }
            if (method == null) {
                Console.Error.WriteLine("ERROR: Static method '{0}' not found in any referenced assembly!", methodName);
                System.Environment.Exit(1);
            }
            return null;
        }

        public void MarkSequencePoint(LexicalInfo lexinfo) {
            if (this.Options.Debug && lexinfo.StartLine != 0) {
                this.ILGenerator.MarkSequencePoint(DebugWriter, lexinfo.StartLine, lexinfo.StartColumn, lexinfo.EndLine, lexinfo.EndColumn);
                this.ILGenerator.Emit(OpCodes.Nop);
            }
        }
    }
}