namespace GliderUI.Common;

public static class TypeMapping
{
    public enum MappingDirection
    {
        ServerToClient,
        ClientToServer,
    };

    public static void Init(
        MappingDirection direction,
        string clientNamespace)
    {
        ObjectTypeMapping.Get().Init(direction, clientNamespace);
    }

    public static void Term()
    {
    }
}
