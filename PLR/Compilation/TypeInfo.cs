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

namespace PLR.Compilation {

    public class TypeInfo {
        private TypeBuilder _type;
        private List<FieldBuilder> _fields = new List<FieldBuilder>();
        private Dictionary<string, LocalBuilder> _locals = new Dictionary<string, LocalBuilder>();
        private ConstructorBuilder _constructor;
        private List<string> _constructorParamNames = new List<string>();
        private bool _isRestricted;
        private bool _isPreProcessed;

        public bool IsRestricted {
            get { return _isRestricted; }
            set { _isRestricted = value; }
        }

        public bool IsPreProcessed {
            get { return _isPreProcessed; }
            set { _isPreProcessed = value; }
        }
        
        public TypeBuilder Builder {
            get { return _type; }
            set { _type = value; }
        }

        public string Name {
            get { return _type.FullName; }
        }

        public List<string> ConstructorParameters {
            get { return _constructorParamNames; }
        }
        public ConstructorBuilder Constructor {
            get { return _constructor; }
            set { _constructor = value; }
        }

        public Dictionary<string, LocalBuilder> Locals {
            get { return _locals; }
        }
        public LocalBuilder GetLocal(string name) {
            if (_locals.ContainsKey(name)) {
                return _locals[name];
            }
            return null;
        }


        public List<FieldBuilder> Fields {
            get { return _fields; }
        }

        public void AddField(FieldBuilder field) {
            _fields.Add(field);
        }

        public FieldBuilder GetField(string name) {
            foreach (FieldBuilder field in _fields) {
                if (field.Name == name) {
                    return field;
                }
            }
            return null;
        }
    }
}
