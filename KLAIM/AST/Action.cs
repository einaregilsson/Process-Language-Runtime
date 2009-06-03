/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using PLR.AST.Expressions;
using System.Text;

namespace KLAIM.AST {

    public abstract class Action : PLR.AST.Actions.Action {
        public string Locality { get; set; }
        public Expression At {
            get {
                return (Expression)_children[0];
            }
            set {
                _children[0] = value;
            }
        }

        public Action() : base("klaimaction") {
            _children.Add(new PLRString("DUMMY"));
        }

        public override System.Collections.Generic.List<Variable> ReadVariables {
            get {
                return FindReadVariables(this);
            }
        }

        public void AddExpression(Expression exp) {
            _children.Add(exp);
        }
        public override string ToString() {
            var sb = new StringBuilder();
            if (this is InAction) {
                sb.Append("in");
            } else if (this is OutAction) {
                sb.Append("out");
            } else if (this is ReadAction) {
                sb.Append("read");
            }
            sb.Append("(");
            sb.Append(_children[1]);
            for (int i = 2; i < _children.Count; i++) {
                sb.Append(", ");
                sb.Append(_children[i]);
            }
            sb.Append(")@");
            sb.Append(At);
            return sb.ToString();
        }
    }
}
