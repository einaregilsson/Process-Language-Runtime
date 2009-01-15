using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WC {
    interface IWhileCompiler {

        void Ident(string ident);
        void Number(int nr);
        void Boolean(bool b);
        void Skip();
        void VarDec(string name);
        void BlockBegin();
        void BlockEnd();
        void Read(string variable);
        void WriteArithmetic();
        void Assign(string variable);
        void UnaryOp(string op);
        void BinaryOp(string op);
    }
}
