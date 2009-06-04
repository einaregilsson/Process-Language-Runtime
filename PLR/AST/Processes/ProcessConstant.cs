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
using System.Reflection.Emit;
using PLR.Analysis;
using PLR.Analysis.Expressions;
using PLR.Compilation;
using PLR.Runtime;

namespace PLR.Analysis.Processes {
    public class ProcessConstant : Process {
        
        public ProcessConstant(string name) {
            _name = name;
            _children.Add(new ExpressionList());
        }

        private string _name;
        public string Name { get { return _name; } }

        public override List<Variable> ReadVariables {
            get { return FindReadVariables(this.Expressions); }
        }
        
        public override List<Process> FlowsTo {
            get {
                return new List<Process>();
            }
        }

        public ExpressionList Expressions { get { return (ExpressionList)_children[2]; } }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public override bool Equals(object obj) {
            if (!(obj is ProcessConstant)) {
                return false;
            }
            ProcessConstant other = (ProcessConstant)obj;
            return other.Name == this.Name && other.Expressions.Count == this.Expressions.Count;
        }

        public override int GetHashCode() {
            return (this.Name + this.Expressions.Count).GetHashCode();

        }
        public string FullName {
            get {
                if (this.Expressions.Count > 0) {
                    return this.Name + "_" + this.Expressions.Count;
                }
                return this.Name;
            }
        }

        public override void Compile(CompileContext context) {

            foreach (Expression exp in this.Expressions) {
                exp.Compile(context);
                if (exp is ArithmeticExpression) {
                    context.ILGenerator.Emit(OpCodes.Box, typeof(Int32));
                }
            }
            context.MarkSequencePoint(this.LexicalInfo);
            EmitRunProcess(context, context.GetType(this.FullName), false, LexicalInfo, false);
        }

        public override string ToString() {
            return this.Name;
        }

    }
}
