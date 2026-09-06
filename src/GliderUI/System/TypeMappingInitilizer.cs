using RpcUIShell.Core;

namespace GliderUI;

internal static partial class TypeMappingInitializer
{
    private static bool s_isInitialized;

    public static void Init()
    {
        if (s_isInitialized)
            return;

        ObjectTypeMapping.Get().Init(
            ObjectTypeMapping.MappingDirection.ClientToServer,
            "GliderUI");

        InitEnumTypeMapping();
        InitObjectTypeMapping();
        s_isInitialized = true;
    }
}
