using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST.Expressions;

namespace PLR.AST.Interfaces {
    public interface IVariableAssignment {
        List<Variable> AssignedVariables { get; }
    }
}
