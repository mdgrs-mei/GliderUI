using GliderUI.Common;

namespace GliderUI.Server;

internal static partial class TypeMappingInitializer
{
    public static void Init()
    {
        ObjectTypeMapping.Get().Init(
            ObjectTypeMapping.MappingDirection.ServerToClient,
            "GliderUI");

        InitEnumTypeMapping();
        InitObjectTypeMapping();
    }
}
