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
using PLR.Compilation;


namespace PLR.AST.ActionHandling {
    public class CustomPreprocess : PreProcessActions {

        public CustomPreprocess(string methodName) {
            _methodName = methodName;
        }
        private string _methodName;
        public string MethodName { get { return _methodName; } }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public override void Compile(CompileContext context) {
            MethodInfo newMethod = context.GetMethod(_methodName);
            base.OverridePreProcess(context, newMethod);
        }
    }
}
