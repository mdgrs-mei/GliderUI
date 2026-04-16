using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace GliderUI.Server;

internal sealed class DataSourceTypeInfo : TypeInfo
{
    private readonly PropertyInfo[] _dynamicProperties;

    public DataSourceTypeInfo(DataSource dataSource)
    {
        List<PropertyInfo> dynamicProperties = [];
        foreach (string memberName in dataSource.GetMemberNames())
        {
            object? member = dataSource.GetMember(memberName);
            DataSourcePropertyInfo propertyInfo = new(
                memberName,
                member?.GetType() ?? typeof(object));

            dynamicProperties.Add(propertyInfo);
        }
        _dynamicProperties = [.. dynamicProperties];
    }

    public override Assembly Assembly { get => typeof(DataSource).Assembly; }
    public override string? AssemblyQualifiedName
    {
        get => typeof(DataSource).AssemblyQualifiedName;
    }
    public override Type? BaseType { get => typeof(DataSource).BaseType; }
    public override string? FullName
    {
        get => typeof(DataSource).FullName;
    }
    public override Guid GUID { get => typeof(DataSource).GUID; }
    public override Module Module { get => typeof(DataSource).Module; }
    public override string? Namespace { get => typeof(DataSource).Namespace; }
    public override Type UnderlyingSystemType { get => typeof(DataSource); }
    public override string Name { get => nameof(DataSource); }

    public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
    {
        return typeof(DataSource).GetConstructors(bindingAttr);
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        return typeof(DataSource).GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return typeof(DataSource).GetCustomAttributes(attributeType, inherit);
    }

    public override Type? GetElementType()
    {
        return typeof(DataSource).GetElementType();
    }

    public override EventInfo? GetEvent(string name, BindingFlags bindingAttr)
    {
        return typeof(DataSource).GetEvent(name, bindingAttr);
    }

    public override EventInfo[] GetEvents(BindingFlags bindingAttr)
    {
        return typeof(DataSource).GetEvents(bindingAttr);
    }

    public override FieldInfo? GetField(string name, BindingFlags bindingAttr)
    {
        return typeof(DataSource).GetField(name, bindingAttr);
    }

    public override FieldInfo[] GetFields(BindingFlags bindingAttr)
    {
        return typeof(DataSource).GetFields(bindingAttr);
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
    public override Type? GetInterface(string name, bool ignoreCase)
    {
        return typeof(DataSource).GetInterface(name, ignoreCase);
    }

    public override Type[] GetInterfaces()
    {
        return typeof(DataSource).GetInterfaces();
    }

    public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
    {
        return typeof(DataSource).GetMembers(bindingAttr);
    }

    public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
    {
        return typeof(DataSource).GetMethods(bindingAttr);
    }

    public override Type? GetNestedType(string name, BindingFlags bindingAttr)
    {
        return typeof(DataSource).GetNestedType(name, bindingAttr);
    }

    public override Type[] GetNestedTypes(BindingFlags bindingAttr)
    {
        return typeof(DataSource).GetNestedTypes(bindingAttr);
    }

    public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
    {
        return _dynamicProperties;
    }

    public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target, object?[]? args, ParameterModifier[]? modifiers, CultureInfo? culture, string[]? namedParameters)
    {
        return typeof(DataSource).InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return typeof(DataSource).IsDefined(attributeType, inherit);
    }

    protected override TypeAttributes GetAttributeFlagsImpl()
    {
        return typeof(DataSource).Attributes;
    }

    protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers)
    {
        return typeof(DataSource).GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
    }

    protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers)
    {
        return typeof(DataSource).GetMethod(name, bindingAttr, binder, callConvention, types!, modifiers);
    }

    protected override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[]? types, ParameterModifier[]? modifiers)
    {
        foreach (PropertyInfo dynamicProperty in _dynamicProperties)
        {
            // Make binding case-insensitive as variable names are case-insensitive in PowerShell.
            if (dynamicProperty.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return dynamicProperty;
            }
        }
        return null;
    }

    protected override bool HasElementTypeImpl()
    {
        return typeof(DataSource).HasElementType;
    }

    protected override bool IsArrayImpl()
    {
        return typeof(DataSource).IsArray;
    }

    protected override bool IsByRefImpl()
    {
        return typeof(DataSource).IsByRef;
    }

    protected override bool IsCOMObjectImpl()
    {
        return typeof(DataSource).IsCOMObject;
    }

    protected override bool IsPointerImpl()
    {
        return typeof(DataSource).IsPointer;
    }

    protected override bool IsPrimitiveImpl()
    {
        return typeof(DataSource).IsPrimitive;
    }
}
