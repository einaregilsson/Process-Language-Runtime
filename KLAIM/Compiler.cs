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
using System.Linq;
using System.Text;
using PLR.AST;
using PLR.AST.Expressions;
using PLR.Compilation;
using KLAIM.Runtime;
using System.Reflection.Emit;

namespace KLAIM {
    public class Compiler {
        private List<TupleInfo> tuples;
        private ProcessSystem processes;

        public void Compile(List<TupleInfo> tuples, ProcessSystem processes, CompileOptions options) {
            this.processes = processes;
            this.processes.MeetTheParents();
            this.tuples = tuples;
            this.processes.MainMethodStart += new CompileEventHandler(CompileTupleSpaces);
            PLRString.DisplayWithoutQuotes = true; //To make localities look right...

            processes.Compile(options);
        }

        void CompileTupleSpaces(CompileContext context) {
            ILGenerator il = context.ILGenerator;
            LocalBuilder loc = il.DeclareLocal(typeof(Locality));
            LocalBuilder arr = il.DeclareLocal(typeof(object[]));
            this.tuples.Sort(delegate(TupleInfo t1, TupleInfo t2) {
                return t1.Locality.CompareTo(t2.Locality);
            });
            string lastLocality = "";
            foreach (TupleInfo ti in this.tuples) {
                if (lastLocality != ti.Locality) {
                    il.Emit(OpCodes.Ldstr, ti.Locality);
                    il.Emit(OpCodes.Call, typeof(Net).GetMethod("AddLocality"));
                    il.Emit(OpCodes.Stloc, loc);
                    lastLocality = ti.Locality;
                }
                il.Emit(OpCodes.Ldc_I4, ti.Items.Count);
                il.Emit(OpCodes.Newarr, typeof(object));
                il.Emit(OpCodes.Stloc, arr);


                for (int i = 0; i < ti.Items.Count; i++) {
                    object elem = ti.Items[i];
                    il.Emit(OpCodes.Ldloc, arr);
                    il.Emit(OpCodes.Ldc_I4, i);
                    if (elem is int) {
                        il.Emit(OpCodes.Ldc_I4, (int)elem);
                        il.Emit(OpCodes.Box, typeof(int));
                    } else if (elem is string) {
                        il.Emit(OpCodes.Ldstr, (string) elem);
                    }
                    il.Emit(OpCodes.Stelem_Ref);
                }
                il.Emit(OpCodes.Ldloc, loc);
                il.Emit(OpCodes.Ldloc, arr);
                il.Emit(OpCodes.Newobj, typeof(Tuple).GetConstructors()[0]);
                il.Emit(OpCodes.Call, typeof(Locality).GetMethod("Out"));
            }
        }
    }
}
