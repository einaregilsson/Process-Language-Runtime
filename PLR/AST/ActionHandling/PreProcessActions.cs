/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System.Collections.Generic;
using System.Reflection;
using System;
using System.Reflection.Emit;
using PLR.Compilation;
using PLR.Runtime;

namespace PLR.AST.ActionHandling {
    public abstract class PreProcessActions : Node {

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        protected void OverridePreProcess(CompileContext context, MethodInfo newMethod) {
            MethodInfo baseGetPreProcess = typeof(ProcessBase).GetMethod("get_PreProcess");
            MethodBuilder getPreProcess = context.Type.Builder.DefineMethod("get_PreProcess", MethodAttributes.Public | MethodAttributes.Virtual, typeof(PreProcessAction), new Type[] { });

            ILGenerator il = getPreProcess.GetILGenerator();
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldftn, newMethod);
            il.Emit(OpCodes.Newobj, typeof(PreProcessAction).GetConstructors()[0]);
            il.Emit(OpCodes.Ret);
            context.Type.Builder.DefineMethodOverride(getPreProcess, baseGetPreProcess);
        }
    }
}
