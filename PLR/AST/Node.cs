using System.Collections.Generic;

namespace PLR.AST {

    public abstract class Node : IEnumerable<Node> {

        public abstract void Accept(AbstractVisitor visitor);
        //Source file information

        protected int _line;
        public int Line
        {
            get { return _line; }
            set { _line = value; }
        }

        protected int _col;
        public int Column
        {
            get { return _col; }
            set { _col = value; }
        }

        protected int _pos;
        public int Position
        {
            get { return _pos; }
            set { _pos = value; }
        }

        protected int _length;
        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }

        protected int _parenCount;
        public int ParenCount
        {
            get { return _parenCount; }
            set { _parenCount = value; }
        }

        public bool HasParens { get { return ParenCount > 0; } }

        protected List<Node> _children = new List<Node>();
        public void SetPos(int line, int col, int length, int pos)
        {
            _line = line;
            _col = col;
            _length = length;
            _pos = pos;
        }

        //protected static Formatter formatter = new Formatter();

        public virtual int Count
        {
            get { return _children.Count; }
        }
        public virtual List<Node> ChildNodes {
            get { return _children; }
        }

        public Node this[int index]{
            get
            {
                return _children[index];
            }
        }

        public override string ToString() {
            return null;
            //return formatter.Format(this);
        }

        public virtual IEnumerator<Node> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
