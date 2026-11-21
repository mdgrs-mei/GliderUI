using RpcUIShell.Core;

namespace GliderUI.Server;

internal static partial class TypeMappingInitializer
{
    public static void Init()
    {
        TypeMapping.Get().Init(
            TypeMapping.MappingDirection.ServerToClient,
            "GliderUI");

        InitMapping();
    }
}
