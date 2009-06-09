/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using PLR.AST.Actions;
using PLR.AST.Expressions;
using PLR.AST;
using PLR.Compilation;
using KLAIM.Runtime;
using PLR.Runtime;

namespace KLAIM.AST {

    /// <summary>
    /// Base class for In and Read actions, since they otherwise would
    /// contain the exact same code exact for the name of the method called...
    /// </summary>
    public abstract class InputAction : KLAIM.AST.Action {

        private string _methodName;
        public InputAction(string methodName) {
            _methodName = methodName;
        }

        public override System.Collections.Generic.List<Variable> AssignedVariables {
            get {
                var list = new List<Variable>();
                foreach (Expression exp in this.ChildNodes) {
                    if (exp is VariableBinding) {
                        list.Add((Variable)exp);
                    }
                }
                return list;
            }
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
            LocalBuilder tuple = il.DeclareLocal(typeof(Tuple));

            if (this.At is Variable && ((Variable)this.At).Name == "self") {
                il.Emit(OpCodes.Ldstr, this.Locality);
            } else {
                this.At.Compile(context);
            }
            il.Emit(OpCodes.Call, typeof(Net).GetMethod("GetLocality"));
            il.Emit(OpCodes.Stloc, loc);

            il.Emit(OpCodes.Ldc_I4, this.ChildNodes.Count - 1);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, arr);


            for (int i = 1; i < this.ChildNodes.Count; i++) {
                Node node = this.ChildNodes[i];
                il.Emit(OpCodes.Ldloc, arr);
                il.Emit(OpCodes.Ldc_I4, i - 1);
                node.Compile(context);
                if (node is ArithmeticExpression) {
                    il.Emit(OpCodes.Box, typeof(int));
                }
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldloc, loc);
            il.Emit(OpCodes.Ldloc, arr);
            il.Emit(OpCodes.Call, typeof(Locality).GetMethod(_methodName));

            //We might be blocked at this location for a very long time...

            il.Emit(OpCodes.Stloc, tuple);
            //Now lets bind our variables... if they are used in the process
            for (int i = 1; i < this.ChildNodes.Count; i++) {
                if (_children[i] is VariableBinding ) {
                    VariableBinding var = (VariableBinding)_children[i];
                    if (context.Options.Optimize && !var.IsUsed) {
                        continue;
                    }

                    //Get the value to assign to it...
                    il.Emit(OpCodes.Ldloc, tuple);
                    il.Emit(OpCodes.Ldc_I4, i - 1);
                    il.Emit(OpCodes.Call, typeof(Tuple).GetMethod("GetValueAt"));

                    //...and assign it
                    LocalBuilder bindLoc = context.Type.GetLocal(var.Name);
                    if (bindLoc == null) {
                        bindLoc = il.DeclareLocal(typeof(object));
                        context.Type.Locals.Add(var.Name, bindLoc);
                        if (context.Options.Debug) {
                            bindLoc.SetLocalSymInfo(var.Name);
                        }
                    }
                    il.Emit(OpCodes.Stloc, bindLoc);
                }
            }
            //Now lets notify our parent that this actions was just executed...
            base.NotifyParents(context);

            //..and lets notify the process that generated this tuple that it has been removed
            //from a tuple space...
            if (this is InAction) {
                Label afterNotify = il.DefineLabel();
                il.Emit(OpCodes.Ldloc, tuple);
                il.Emit(OpCodes.Call, typeof(Tuple).GetMethod("get_Subscriber"));
                il.Emit(OpCodes.Brfalse, afterNotify); //if subscriber != null
                {
                    il.Emit(OpCodes.Ldloc, tuple);
                    il.Emit(OpCodes.Call, typeof(Tuple).GetMethod("get_Subscriber"));
                    il.Emit(OpCodes.Ldloc, tuple);
                    il.Emit(OpCodes.Call, typeof(Tuple).GetMethod("get_GeneratingActionNr"));
                    il.Emit(OpCodes.Callvirt, typeof(IActionSubscriber).GetMethod("NotifyAction"));
                }
                il.MarkLabel(afterNotify);
            }

            //Now lets print out the net for fun...

            il.EmitWriteLine("************** CURRENT TUPLES **************");
            il.Emit(OpCodes.Call, typeof(Net).GetMethod("Display"));
            il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            //TODO: Notify possible replicated process

        }
    }
}
