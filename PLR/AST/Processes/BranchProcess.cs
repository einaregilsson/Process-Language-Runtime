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
using PLR.AST.Expressions;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Processes {
    public class BranchProcess : Process {


        public BranchProcess(Expression exp, Process ifBranch, Process elseBranch) {
            _children.Add(exp);
            _children.Add(ifBranch);
            _children.Add(elseBranch);
        }

        public Expression Expression {
            get { return (Expression)_children[2]; }
        }

        public Process IfBranch {
            get { return (Process)_children[3]; }
        }

        public Process ElseBranch{
            get { return (Process)_children[4]; }
        }

        protected override bool WrapInTryCatch {
            get { return false; } 
        }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            Label elseStart = context.ILGenerator.DefineLabel();
            Label elseEnd = context.ILGenerator.DefineLabel();
            this.Expression.Compile(context);
            context.ILGenerator.Emit(OpCodes.Brfalse, elseStart);
            IfBranch.Compile(context);
            context.ILGenerator.Emit(OpCodes.Br, elseEnd);
            context.ILGenerator.MarkLabel(elseStart);
            ElseBranch.Compile(context);
            context.ILGenerator.MarkLabel(elseEnd);
        }

        public override string ToString() {
            return "if " + Expression + " then " + IfBranch + " else " + ElseBranch + " fi";
        }
    }
}
