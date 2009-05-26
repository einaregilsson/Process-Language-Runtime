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

namespace KLAIM.Runtime {
    public class KlaimException : Exception{
        public KlaimException(string msg) : base(msg) { }
        public KlaimException() { }
    }
}
