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
        private List<FieldBuilder> _fields = new List<FieldBuilder>();
        private Dictionary<string, LocalBuilder> _locals = new Dictionary<string, LocalBuilder>();
        private ConstructorBuilder _constructor;
        private List<string> _constructorParamNames = new List<string>();

        public bool IsRestricted { get; set;}
        public bool IsPreProcessed { get; set; }

        //Implementations can set this to true if they want a particular process
        //to live on after it has spawned its child processes. If set to true
        //the process will set itself as the parent process of all its children.
        public bool MustLiveOn { get; set; } 
        public TypeBuilder Builder { get; set;}
        public ConstructorBuilder Constructor { get; set; }

        public string Name {
            get { return Builder.FullName; }
        }

        public List<string> ConstructorParameters {
            get { return _constructorParamNames; }
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
