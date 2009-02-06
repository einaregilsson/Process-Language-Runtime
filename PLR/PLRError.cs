using System;
using System.Collections.Generic;

using System.Text;

namespace PLR
{
    public class PLRError
    {
        public int Line;// { get; private set; }
        public int Col;// { get; private set; }
        public string Message;// { get; private set; }

        public PLRError(string message, int line, int col)
        {
            this.Message = message;
            this.Line = line;
            this.Col = col;
        }

        public override string ToString()
        {
            return string.Format("-- ({0},{1}) {2}", Line, Col, Message);
        }
    }
}
