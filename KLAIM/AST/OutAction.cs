/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using PLR.Analysis.Actions;
using PLR.Analysis.Expressions;
using PLR.Analysis;
using PLR.Compilation;
using PLR.Runtime;
using KLAIM.Runtime;

namespace KLAIM.AST {
    
    public class OutAction : KLAIM.AST.Action {
        
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            Type procType = typeof(ProcessBase);
            ILGenerator il = context.ILGenerator;

            EmitDebug("Preparing to sync now...", context);
            LocalBuilder syncObject = il.DeclareLocal(typeof(KLAIMAction));

            il.Emit(OpCodes.Ldarg_0); //this
            il.Emit(OpCodes.Ldarg_0); //this
            il.Emit(OpCodes.Ldstr, this.ToString());
            il.Emit(OpCodes.Newobj, typeof(KLAIMAction).GetConstructors()[0]);
            il.Emit(OpCodes.Call, SyncMethod);

            //..and here we actually do something...
            context.MarkSequencePoint(this.LexicalInfo);

            LocalBuilder loc = il.DeclareLocal(typeof(Locality));
            LocalBuilder arr = il.DeclareLocal(typeof(object[]));

            if (this.At is Variable && ((Variable)this.At).Name == "self") {
                il.Emit(OpCodes.Ldstr, this.Locality);            
            } else {
                this.At.Compile(context);
            }
            il.Emit(OpCodes.Call, typeof(Net).GetMethod("GetLocality"));
            il.Emit(OpCodes.Stloc, loc);

            il.Emit(OpCodes.Ldc_I4, this.ChildNodes.Count-1);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, arr);


            for (int i = 1; i < this.ChildNodes.Count; i++) {
                Node node = this.ChildNodes[i];
                il.Emit(OpCodes.Ldloc, arr);
                il.Emit(OpCodes.Ldc_I4, i-1);
                node.Compile(context);
                if (node is ArithmeticExpression) {
                    il.Emit(OpCodes.Box, typeof(int));
                }
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldloc, loc);
            il.Emit(OpCodes.Ldloc, arr);
            il.Emit(OpCodes.Call, typeof(Locality).GetMethod("Out"));

            //Now lets print out the net for fun...

            il.EmitWriteLine("************** CURRENT TUPLES **************");
            il.Emit(OpCodes.Call, typeof(Net).GetMethod("Display"));
            il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new Type[] {typeof(string)}));
        }
    }
}
