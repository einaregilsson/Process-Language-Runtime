/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace PLR.Runtime {
    public static class BuiltIns {
        public static bool RestrictAll(IAction action) {
            return true;
        }

        public static void Print(object s) {
            Console.WriteLine(s);
        }

        public static int GetFive() { return 5; }

        public static void Println(string s, int a) {
            Console.WriteLine(s + a);
        }
    }
}
