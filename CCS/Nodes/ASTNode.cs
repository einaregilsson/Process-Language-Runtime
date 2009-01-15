using CCS.Formatters;
using System.Collections.Generic;

namespace CCS.Nodes {

    public abstract class ASTNode {
        public int Line { get; set; }
        public int Col { get; set; }
        public int Pos { get; set; }
        public int Length { get; set; }
        public void SetPos(CCS.Parsing.Token t) {
            Line = t.line;
            Col = t.col;
            Pos = t.pos;
            Length = t.val.Length;
        }

        public void CopyPos(ASTNode startnode, CCS.Parsing.Token current) {
            this.Line = startnode.Line;
            this.Col = startnode.Col;
            this.Pos = startnode.Pos;
            this.Length = current.pos - startnode.Pos;
        }

        protected static Formatter formatter = new Formatter();

        public int ParenCount { get; set; }

        public virtual List<ASTNode> GetChildren() { 
            return new List<ASTNode>(); 
        }

        public override string ToString() {
            return formatter.Format(this);
        }
    }
}
