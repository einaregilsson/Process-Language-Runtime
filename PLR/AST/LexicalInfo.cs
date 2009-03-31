using System;
using System.Collections.Generic;
using System.Text;

namespace PLR.AST {
    public class LexicalInfo {
        public LexicalInfo(){}

        private int _startLine;
        public int StartLine
        {
            get { return _startLine; }
            set { _startLine = value; }
        }

        private int _startCol;
        public int StartColumn
        {
            get { return _startCol; }
            set { _startCol = value; }
        }
        private int _endLine;
        public int EndLine
        {
            get { return _endLine; }
            set { _endLine = value; }
        }

        private int _endCol;
        public int EndColumn
        {
            get { return _endCol; }
            set { _endCol = value; }
        }
    }
}
