using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CCS.Nodes {
    public class Subscript : ASTNode, IEnumerable<ArithmeticExpression> {

        private List<ArithmeticExpression> list = new List<ArithmeticExpression>();

        public void Add(ArithmeticExpression exp) {
            list.Add(exp);
        }

        public int Count {
            get { return list.Count; }
        }

        public ArithmeticExpression this[int index] {
            get { return list[index]; }
        }

        #region IEnumerable<ArithmeticExpression> Members

        public IEnumerator<ArithmeticExpression> GetEnumerator() {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return list.GetEnumerator();
        }

        #endregion

        public override List<ASTNode> GetChildren() {
            var l = new List<ASTNode>();
            foreach (ASTNode a in this) {
                l.Add(a);
            }
            return l;
        }

    }
}
