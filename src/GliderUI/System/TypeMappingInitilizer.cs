using RpcUIShell.Core;

namespace GliderUI;

internal static partial class TypeMappingInitializer
{
    public static void Init()
    {
        ObjectTypeMapping.Get().Init(
            ObjectTypeMapping.MappingDirection.ClientToServer,
            "GliderUI");

        InitEnumTypeMapping();
        InitObjectTypeMapping();
    }
}
