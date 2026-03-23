using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using GliderUI.Common;

namespace GliderUI;

#pragma warning disable CA1710 // Identifiers should have correct suffix
public class DataSource : DynamicObject, IDictionary<string, object?>, IGliderUIObject
#pragma warning restore CA1710
{
    private readonly HashSet<string> _memberNames = [];
    private readonly HashSet<string> _originalMemberNames = [];

    public ObjectId GliderUIObjectId { get; protected set; } = new();

    public ICollection<string> Keys
    {
        get => [.. _originalMemberNames];
    }

    public ICollection<object?> Values
    {
        get
        {
            List<object?> values = new(_memberNames.Count);
            foreach (string name in _memberNames)
            {
                var value = GetMember(name);
                values.Add(value);
            }
            return values;
        }
    }

    public int Count
    {
        get => _memberNames.Count;
    }

    public bool IsReadOnly
    {
        get => false;
    }

    public object? this[string key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);
            _ = TryGetMemberWithOriginalName(key, out object? value);
            return value;
        }
        set => SetMember(key, value);
    }

    public DataSource()
    {
        GliderUIObjectId = CommandClient.Get().CreateObject(
            "GliderUI.Server.DataSource, GliderUI.Server",
            this);
    }

    public DataSource(Hashtable hashtable)
        : this()
    {
        ArgumentNullException.ThrowIfNull(hashtable);

        foreach (DictionaryEntry keyValue in hashtable)
        {
            SetMemberWithOriginalName((string)keyValue.Key, keyValue.Value);
        }
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        return _originalMemberNames.AsEnumerable();
    }

    public override bool TryGetMember(
        GetMemberBinder binder, out object? result)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return TryGetMemberWithOriginalName(binder.Name, out result);
    }

    public override bool TrySetMember(
        SetMemberBinder binder, object? value)
    {
        ArgumentNullException.ThrowIfNull(binder);
        SetMemberWithOriginalName(binder.Name, value);
        return true;
    }

    private bool TryGetMemberWithOriginalName(string originalMemberName, out object? value)
    {
        string memberName = originalMemberName.ToUpperInvariant();
        if (_memberNames.Contains(memberName))
        {
            value = GetMember(memberName);
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    private object? GetMember(string memberName)
    {
        object? result = CommandClient.Get().InvokeMethodAndGetResult<object?>(
            GliderUIObjectId,
            "GliderUI.Server.DataSource, GliderUI.Server",
            "GetMember",
            memberName);

        return result;
    }

    private void SetMemberWithOriginalName(string originalMemberName, object? value)
    {
        string memberName = originalMemberName.ToUpperInvariant();
        _ = _memberNames.Add(memberName);
        _ = _originalMemberNames.Add(originalMemberName);
        SetMember(memberName, value);
    }

    private void SetMember(string memberName, object? value)
    {
        CommandClient.Get().InvokeMethod(
            GliderUIObjectId,
            "GliderUI.Server.DataSource, GliderUI.Server",
            "SetMember",
            memberName,
            value);
    }

    public void Add(string key, object? value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (ContainsKey(key))
        {
            throw new ArgumentException($"An element with the key '{key}' already exists in the DataSource.");
        }

        SetMemberWithOriginalName(key, value);
    }

    public bool ContainsKey(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        string memberName = key.ToUpperInvariant();
        return _memberNames.Contains(memberName);
    }

    public bool Remove(string key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return TryGetMemberWithOriginalName(key, out value);
    }

    public void Add(KeyValuePair<string, object?> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<string, object?> item)
    {
        if (TryGetValue(item.Key, out object? value))
        {
            if (value is null)
            {
                return item.Value is null;
            }
            return value.Equals(item.Value);
        }
        return false;
    }

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        GetKeyValueList().CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, object?> item)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return GetKeyValueList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private List<KeyValuePair<string, object?>> GetKeyValueList()
    {
        List<KeyValuePair<string, object?>> list = new(_originalMemberNames.Count);
        foreach (string originalMemberName in _originalMemberNames)
        {
            _ = TryGetMemberWithOriginalName(originalMemberName, out object? value);
            list.Add(new KeyValuePair<string, object?>(originalMemberName, value));
        }
        return list;
    }
}
