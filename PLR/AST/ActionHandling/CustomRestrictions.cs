using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.ActionHandling {
    public class CustomRestrictions : ActionRestrictions{

        public CustomRestrictions(string methodName) {
            _methodName = methodName;
        }
        private string _methodName;

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            MethodInfo newMethod = context.GetMethod(_methodName);
            base.OverrideRestrict(context, newMethod);
        }
    }
}
