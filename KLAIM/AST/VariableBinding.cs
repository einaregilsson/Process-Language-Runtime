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
using PLR.AST.Expressions;
using System.Reflection.Emit;

namespace KLAIM.AST {
    public class VariableBinding : Expression{
        public string Name { get; set; }
        public VariableBinding(string name) {
            this.Name = name;
        }
        public override Type Type {
            get { throw new NotImplementedException(); }
        }

        public override string ToString() {
            return "!" + Name;
        }

        public override void Compile(PLR.Compilation.CompileContext context) {
            context.ILGenerator.Emit(OpCodes.Ldnull);
        }
    }
}
