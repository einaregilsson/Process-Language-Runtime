using PLR;

namespace External {
    public static class CustomFunctions {
        public static IAction MyProcess(IAction a) {
            if (!(a is ChannelSync)) {
                return a;
            }
            ChannelSync c = (ChannelSync)a;
            c.Name += "ITWORKS";
            return c;
        }

        public static bool RestrictIT(IAction a) {
            return true;
        }
    }
}
