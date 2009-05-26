/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System.Reflection.Emit;
using PLR.Compilation;

namespace PLR.AST.Processes {
    public class NilProcess : Process{
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
            EmitDebug("Turned into 0",context);
            if (context.Options.Debug && LexicalInfo.StartLine != 0) {
                context.MarkSequencePoint(LexicalInfo);
                context.ILGenerator.Emit(OpCodes.Nop);
            }
        }

        public override string ToString() {
            return "0";
        }
    }
}
