/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace PLR.AST {
    public class LexicalInfo {
        public LexicalInfo() { }

        public int s;
        public int StartLine {
            get { return s; }
            set {
                if (value == 0) Console.WriteLine("SET TO 0" + new System.Diagnostics.StackTrace()); s = value;
            }
        }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public override string ToString() {
            return string.Format("({0},{1},{2},{3})", StartLine, StartColumn, EndLine, EndColumn);
        }
    }
}
