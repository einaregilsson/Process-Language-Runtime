using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;


namespace PLR.AST.ActionHandling {
    public class CustomPreprocess : PreProcessActions {

        public CustomPreprocess(string methodName) {
            _methodName = methodName;
        }
        private string _methodName;

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            MethodInfo newMethod = context.GetMethod(_methodName);
            base.OverridePreProcess(context, newMethod);
        }
    }
}
