using RpcUIShell.Core;

namespace GliderUI;

internal static partial class TypeMappingInitializer
{
    private static bool s_isInitialized;

    public static void Init()
    {
        if (s_isInitialized)
            return;

        TypeMapping.Get().Init(
            TypeMapping.MappingDirection.ClientToServer,
            "GliderUI");

        InitMapping();
        s_isInitialized = true;
    }
}
