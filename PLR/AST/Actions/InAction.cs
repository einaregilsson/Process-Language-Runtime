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
using PLR.Compilation;
using PLR.Runtime;
using PLR.AST.Expressions;

namespace PLR.AST.Actions {
    public class InAction : Action {
        public InAction(string name) : base(name) { }

        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
            base.Accept(visitor);
        }

        public void AddVariable(Variable var) {
            _children.Add(var);
        }

        public override void Compile(CompileContext context) {
            Type procType = typeof(ProcessBase);
            ILGenerator il = context.ILGenerator;

            EmitDebug("Preparing to sync now...", context);
            LocalBuilder syncObject = il.DeclareLocal(typeof(ChannelSyncAction));
            il.Emit(OpCodes.Ldarg_0); //this
            il.Emit(OpCodes.Ldstr, Name);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, _children.Count);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Newobj, typeof(ChannelSyncAction).GetConstructor(new Type[] { typeof(string), typeof(ProcessBase), typeof(int), typeof(bool) }));
            il.Emit(OpCodes.Stloc, syncObject);
            il.Emit(OpCodes.Ldloc, syncObject);
            il.Emit(OpCodes.Call, SyncMethod);
            context.MarkSequencePoint(this.LexicalInfo);

            //Save values to variables
            for (int i = 0; i < _children.Count; i++) {
                Variable var = (Variable)_children[i];

                if (context.Options.Optimize && !var.IsUsed) {
                    continue;
                }
                //Get the value to assign to it...
                il.Emit(OpCodes.Ldloc, syncObject);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Call, typeof(ChannelSyncAction).GetMethod("GetValue"));
                //...and assign it
                LocalBuilder local = context.Type.GetLocal(var.Name);
                if (local == null) { //Inactions can be defining occurrences, so just create it...
                    local = context.ILGenerator.DeclareLocal(typeof(object));
                    if (context.Options.Debug) {
                        local.SetLocalSymInfo(var.Name);
                    }
                    context.Type.Locals.Add(var.Name, local);
                }
                il.Emit(OpCodes.Stloc, local);
            }
        }

        public override List<Variable> AssignedVariables {
            get {
                var list = new List<Variable>();
                foreach (Variable v in this.ChildNodes) {
                    list.Add(v);
                }
                return list;
            }
        }

        public override string ToString() {
            return _name;
        }
    }
}
