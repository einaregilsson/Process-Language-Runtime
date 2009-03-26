using System;
using System.Collections.Generic;
using System.Text;

namespace PLR {
    public static class BuiltIns {
        public static bool RestrictAll(IAction action) {
            return true;
        }

        public static void Print(string s) {
            Console.Write(s);
        }

        public static void Println(string s) {
            Console.WriteLine(s);
        }

        public static void Println(string s, int a) {
            Console.WriteLine(s + a);
        }
    }
}
