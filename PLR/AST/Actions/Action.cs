/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
﻿using System.Reflection;
using System.Collections.Generic;
using PLR.Runtime;
using PLR.AST.Expressions;

namespace PLR.AST.Actions {

    public abstract class Action : Node{

        public virtual List<Variable> ReadVariables { get { return new List<Variable>(); } }
        public virtual List<Variable> AssignedVariables { get { return new List<Variable>(); } }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

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
