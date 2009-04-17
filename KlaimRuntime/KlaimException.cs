using System;
using System.Collections.Generic;
using System.Text;

namespace KlaimRuntime {
    public class KlaimException : Exception{
        public KlaimException(string msg) : base(msg) { }
        public KlaimException() { }
    }
}
