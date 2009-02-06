using System.Collections.Generic;

namespace PLR.AST {

    public abstract class Node : IEnumerable<Node> {
        //Source file information
        public int Line;// { get; set; }
        public int Col;// { get; set; }
        public int Pos;// { get; set; }
        public int Length;// { get; set; }
        public int ParenCount;// { get; set; }

        protected List<Node> _children = new List<Node>();

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

        public IEnumerator<Node> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }
    }
}
