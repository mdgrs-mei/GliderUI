using System.ComponentModel;

namespace GliderUI.Server;

#pragma warning disable CA1515 // Consider making public types internal
public sealed class DataSource : INotifyPropertyChanged
#pragma warning restore CA1515
{
    private readonly Dictionary<string, object?> _members = [];
    public event PropertyChangedEventHandler? PropertyChanged;

    internal bool TryGetMember(string memberName, out object? value)
    {
        return _members.TryGetValue(memberName, out value);
    }

    public object? GetMember(string memberName)
    {
        return _members[memberName];
    }

    public void SetMember(string memberName, object? value)
    {
        _members[memberName] = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
}
