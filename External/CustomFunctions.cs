using PLR;
using PLR.Runtime;

namespace External {
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
    }
}
