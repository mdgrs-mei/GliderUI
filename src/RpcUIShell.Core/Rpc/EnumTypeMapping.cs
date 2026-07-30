namespace RpcUIShell.Core;

public class EnumTypeMapping : Singleton<EnumTypeMapping>
{
    private readonly Dictionary<string, string> _map = [];

    public EnumTypeMapping()
    {
    }

    public void InitMapping(IList<(string, string)> list)
    {
        ArgumentNullException.ThrowIfNull(list);

        foreach (var map in list)
        {
            _map.Add(map.Item1, map.Item2);
            _map.Add(map.Item2, map.Item1);
        }
    }

    public bool TryGetValue(string sourceTypeName, out string? targetTypeName)
    {
        return _map.TryGetValue(sourceTypeName, out targetTypeName);
    }

    internal bool TryGetTargetTypeName(Type sourceType, out string? targetTypeName)
    {
        var assemblyName = sourceType.Assembly.GetName().Name;
        return TryGetValue($"{sourceType.FullName}, {assemblyName}", out targetTypeName);
    }
}
