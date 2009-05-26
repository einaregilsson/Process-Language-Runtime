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
using System.Reflection;

namespace PLR {
    static class MethodResolver {

        public static MethodInfo GetMethod(Type t, string name) {

            foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
                if (m.Name == name) {
                    return m;
                }
            }
            return null;
        }
        public static ConstructorInfo GetConstructor(Type t) {
            foreach (ConstructorInfo c in t.GetConstructors()) {
                return c;
            }
            return null;
        }
    }
}
