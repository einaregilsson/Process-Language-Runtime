﻿using System.Collections.Generic;

namespace PLR.AST.Processes {

    public class NonDeterministicChoice : Process {
        
        public new Process this[int index] {
            get { return (Process)_children[index]; }
        }

        public void Add(Process p) {
            _children.Add(p);
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

    }
}