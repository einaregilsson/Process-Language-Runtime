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
using PLR.Analysis;

namespace PLR.Analysis {
    public class Warning {

        public LexicalInfo LexicalInfo { get; set; }
        public string Message { get; set; }
        public Warning(LexicalInfo lex, string msg) {
            this.LexicalInfo = lex;
            this.Message = msg;
        }
    }
}
