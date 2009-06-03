using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST.Expressions;

namespace PLR.AST.Interfaces {
    public interface IVariableReader {
        List<Variable> ReadVariables { get; }
    }
}
