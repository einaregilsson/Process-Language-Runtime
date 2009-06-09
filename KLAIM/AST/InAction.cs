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
using PLR.AST.Actions;
using PLR.AST.Expressions;
using PLR.AST;
using PLR.Compilation;
using KLAIM.Runtime;
using PLR.Runtime;

namespace KLAIM.AST {

    public class InAction : KLAIM.AST.InputAction {
        public InAction()
            : base("In") {
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

    }
}
