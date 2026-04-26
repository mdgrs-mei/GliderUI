namespace GliderUI.Common;

public sealed partial class ObjectTypeMapping : Singleton<ObjectTypeMapping>
{
    public enum MappingDirection
    {
        ServerToClient,
        ClientToServer,
    };

    public MappingDirection Direction { get; set; } = MappingDirection.ClientToServer;

    public string GetTargetTypeName(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        _ = TryGetTargetTypeName(type, out string? targetTypeName);
        if (targetTypeName is null)
        {
            throw new InvalidOperationException($"Object type mapping not found for [{type.FullName}].");
        }
        return targetTypeName;
    }

    internal bool TryGetTargetTypeName(Type sourceType, out string? targetTypeName)
    {
        var assemblyName = sourceType.Assembly.GetName().Name;
        if (sourceType.IsGenericType)
        {
            if (!TryGetValue($"{sourceType.FullName!.Split('[')[0]}, {assemblyName}", out string? thisName))
            {
                targetTypeName = null;
                return false;
            }

            string[] thisNameAndAssembly = thisName!.Split(", ");

            List<string> argNames = [];
            foreach (var argType in sourceType.GetGenericArguments())
            {
                if (!TryGetTargetTypeName(argType, out string? argTargetTypeName))
                {
                    targetTypeName = null;
                    return false;
                }
                argNames.Add($"[{argTargetTypeName}]");
            }

            targetTypeName = $"{thisNameAndAssembly[0]}[{string.Join(", ", argNames)}], {thisNameAndAssembly[1]}";
            return true;
        }
        else
        {
            return TryGetValue($"{sourceType.FullName}, {assemblyName}", out targetTypeName);
        }
    }

    private bool TryGetValue(string sourceTypeName, out string? targetTypeName)
    {
        if (Direction == MappingDirection.ClientToServer)
        {
            if (sourceTypeName.StartsWith("GliderUI", StringComparison.Ordinal))
            {
                return _map.TryGetValue(sourceTypeName, out targetTypeName);
            }
            else
            {
                targetTypeName = sourceTypeName;
                return true;
            }
        }
        else
        {
            return _map.TryGetValue(sourceTypeName, out targetTypeName);
        }
    }
}
