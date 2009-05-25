using System;
using System.Collections.Generic;
using System.Text;

namespace KLAIM.Runtime {
    public class KlaimException : Exception{
        public KlaimException(string msg) : base(msg) { }
        public KlaimException() { }
    }
}
