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

        private static Random _rand = new Random();
        public static int Rand(int max) {
            return _rand.Next(max);
        }

        public static bool Prob(int percent) {
            if (percent < 1 || percent > 100) {
                throw new ArgumentException("Must be between 1 and 99", "percent");
            }
            return _rand.Next(1, 101) <= percent;
        }

        public static void Println(string s, int a) {
            Console.WriteLine(s + a);
        }
    }
}
