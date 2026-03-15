using GliderUI.Common;

namespace GliderUI.Server;

internal static class ObjectValidator
{
    public static void Init()
    {
        Invoker.Get().Validator = IsValid;
    }

    public static void Term()
    {
        Invoker.Get().Validator = null;
    }

    public static bool IsValid(object obj)
    {
        return true;
    }
}
