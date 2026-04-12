using System.Globalization;
using System.Reflection;

namespace GliderUI.Server;

internal sealed class DataSourcePropertyInfo : PropertyInfo
{
    private readonly string _name;
    private readonly Type _propertyType;

    public override PropertyAttributes Attributes { get => PropertyAttributes.None; }
    public override bool CanRead { get => true; }
    public override bool CanWrite { get => true; }
    public override Type PropertyType { get => _propertyType; }
    public override Type? DeclaringType { get => typeof(DataSource); }
    public override string Name { get => _name; }
    public override Type? ReflectedType { get => DeclaringType; }

    public DataSourcePropertyInfo(
        string name,
        Type propertyType)
    {
        _name = name;
        _propertyType = propertyType;
    }

    public override MethodInfo[] GetAccessors(bool nonPublic)
    {
        throw new NotImplementedException();
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        throw new NotImplementedException();
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        throw new NotImplementedException();
    }

    public override MethodInfo? GetGetMethod(bool nonPublic)
    {
        throw new NotImplementedException();
    }

    public override ParameterInfo[] GetIndexParameters()
    {
        throw new NotImplementedException();
    }

    public override MethodInfo? GetSetMethod(bool nonPublic)
    {
        throw new NotImplementedException();
    }

    public override object? GetValue(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture)
    {
        if (obj is DataSource dataSource)
        {
            return dataSource.GetMember(Name);
        }
        else
        {
            throw new ArgumentException($"Object must be of type {typeof(DataSource).FullName}");
        }
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return false;
    }

    public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture)
    {
        if (obj is DataSource dataSource)
        {
            dataSource.SetMember(Name, value);
        }
        else
        {
            throw new ArgumentException($"Object must be of type {typeof(DataSource).FullName}");
        }
    }
}
