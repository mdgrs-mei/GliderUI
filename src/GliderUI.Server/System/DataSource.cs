using System.ComponentModel;
using System.Reflection;

namespace GliderUI.Server;

#pragma warning disable CA1515 // Consider making public types internal
public sealed class DataSource : INotifyPropertyChanged, IReflectableType
#pragma warning restore CA1515
{
    private readonly Dictionary<string, object?> _members = [];
    public event PropertyChangedEventHandler? PropertyChanged;

    public object? GetMember(string memberName)
    {
        return _members[memberName];
    }

    public void SetMember(string memberName, object? value)
    {
        _members[memberName] = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }

    // To make dynamic properties discoverable by Avalonia, implement IReflectableType.
    public TypeInfo GetTypeInfo()
    {
        return new DataSourceTypeInfo(this);
    }

    internal string[] GetMemberNames()
    {
        return [.. _members.Keys];
    }
}
