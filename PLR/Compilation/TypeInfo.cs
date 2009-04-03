using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.Compilation {

    public class TypeInfo {
        private TypeBuilder _type;
        private TypeBuilder _variablesType;
        private List<FieldBuilder> _fields = new List<FieldBuilder>();
        private ConstructorBuilder _constructor;
        private FieldBuilder _variablesField;
        private ConstructorBuilder _variablesConstructor;
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
        
        public FieldBuilder VariablesField {
            get { return _variablesField; }
            set { _variablesField = value; }
        }

        public ConstructorBuilder VariablesConstructor {
            get { return _variablesConstructor; }
            set { _variablesConstructor = value; }
        }

        public TypeBuilder Builder {
            get { return _type; }
            set { _type = value; }
        }

        public string Name {
            get { return _type.FullName; }
        }

        public TypeBuilder Variables {
            get { return _variablesType; }
            set { _variablesType = value; }
        }

        public ConstructorBuilder Constructor {
            get { return _constructor; }
            set { _constructor = value; }
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
