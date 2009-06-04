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

namespace PLR.Analysis {
    public class LexicalInfo {
        public LexicalInfo() { }


        public override bool Equals(object obj) {
            if (!(obj is LexicalInfo)) {
                return false;
            }

            LexicalInfo other = (LexicalInfo)obj;
            return this.ToString() == other.ToString();
        }

        public override int GetHashCode() {
            return this.ToString().GetHashCode();
        }

        public void Copyfrom(LexicalInfo other) {
            this.StartLine = other.StartLine;
            this.StartColumn = other.StartColumn;
            this.EndLine = other.EndLine;
            this.EndColumn = other.EndColumn;
        }

        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public override string ToString() {
            return string.Format("({0},{1},{2},{3})", StartLine, StartColumn, EndLine, EndColumn);
        }
    }
}
