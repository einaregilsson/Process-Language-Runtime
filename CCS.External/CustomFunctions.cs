/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using PLR;
using PLR.Runtime;

namespace CCS.External {
    
    public static class CustomFunctions {
    
        public static IAction MyProcess(IAction a) {
            if (!(a is ChannelSyncAction)) {
                return a;
            }
            ChannelSyncAction c = (ChannelSyncAction)a;
            c.Name += "ITWORKS";
            return c;
        }

        public static bool RestrictIT(IAction a) {
            return true;
        }

        public static void Print(string s) {
            System.Console.WriteLine(s);
        }


        public static void WRITE(string s) {
            System.Console.WriteLine(s);
        }

        public static void WRITE(string s, int arg0) {
            System.Console.WriteLine(s + arg0);
        }
    }
}
