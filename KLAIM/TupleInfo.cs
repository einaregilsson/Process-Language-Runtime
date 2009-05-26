/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KLAIM {
    public class TupleInfo {
        public string Locality { get; set; }
        public List<object> Items { get; set; }
    }
}
