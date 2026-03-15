using GliderUI.ApiExporter;

namespace GliderUI.Generator;

internal class PropertyDef
{
    private readonly ObjectDef _objectDef;
    private readonly MemberDefType _memberDefType;
    private readonly List<ParameterDef>? _indexParameters;
    private readonly bool _hidesBase;
    private readonly bool _isOverride;
    private readonly bool _isVirtual;
    private readonly bool _isAbstract;
    private readonly string _propertyName;

    public readonly TypeDef Type;
    public readonly TypeDef? ExplicitInterfaceType;
    public bool CanRead { get; private set; }
    public bool CanWrite { get; private set; }
    public bool ImplementsInterface { get; private set; }
    public bool IsIndexer
    {
        get => _indexParameters is not null;
    }

    public PropertyDef(
        Api.PropertyDef apiPropertyDef,
        ObjectDef objectDef,
        MemberDefType memberDefType)
    {
        _hidesBase = apiPropertyDef.HidesBase;
        _isOverride = apiPropertyDef.IsOverride;

        // Additinally make abstract methods in classes virtual to provide default implementation because abstract classes need to be instantiated as return values.
        _isVirtual = apiPropertyDef.IsVirtual || (apiPropertyDef.IsAbstract && !objectDef.Type.IsInterface);
        // Remove abstract from methods in classes. Instead, make them virtual.
        _isAbstract = apiPropertyDef.IsAbstract && objectDef.Type.IsInterface;

        _propertyName = apiPropertyDef.Name;

        CanRead = apiPropertyDef.CanRead;
        CanWrite = apiPropertyDef.CanWrite;
        ImplementsInterface = apiPropertyDef.ImplementsInterface;

        _objectDef = objectDef;
        _memberDefType = memberDefType;

        bool useSystemInterfaceName = apiPropertyDef.ImplementsGlobalSystemInterface;
        ExplicitInterfaceType = apiPropertyDef.ExplicitInterfaceType is null ?
            null :
            new TypeDef(apiPropertyDef.ExplicitInterfaceType, useSystemInterfaceName);

        if (apiPropertyDef.IndexParameters is not null)
        {
            foreach (var apiParameterDef in apiPropertyDef.IndexParameters)
            {
                if (_indexParameters is null)
                {
                    _indexParameters = [];
                }
                _indexParameters.Add(new ParameterDef(apiParameterDef, useSystemInterfaceName));
            }
        }

        Type = new TypeDef(apiPropertyDef.Type, useSystemInterfaceName);
    }

    public PropertyDef(
        string name,
        Api.MethodDef getter,
        Api.MethodDef? setter,
        ObjectDef objectDef,
        MemberDefType memberDefType)
    {
        _hidesBase = getter.HidesBase;
        _isOverride = getter.IsOverride;
        _isVirtual = getter.IsVirtual || (getter.IsAbstract && !objectDef.Type.IsInterface); ;
        _isAbstract = getter.IsAbstract && objectDef.Type.IsInterface;
        _propertyName = name;

        CanRead = true;
        CanWrite = setter is not null;
        ImplementsInterface = getter.ImplementsInterface;

        _objectDef = objectDef;
        _memberDefType = memberDefType;

        bool useSystemInterfaceName = getter!.ImplementsGlobalSystemInterface;
        ExplicitInterfaceType = getter.ExplicitInterfaceType is null ? null : new TypeDef(getter.ExplicitInterfaceType, useSystemInterfaceName);

        List<Api.ParameterDef>? indexParameters = getter.Parameters!;
        if (indexParameters is not null)
        {
            foreach (var apiParameterDef in indexParameters)
            {
                if (_indexParameters is null)
                {
                    _indexParameters = [];
                }
                _indexParameters.Add(new ParameterDef(apiParameterDef, useSystemInterfaceName));
            }
        }

        Type = new TypeDef(getter.ReturnType!, useSystemInterfaceName);
    }

    public bool IsSupported()
    {
        if (!Type.IsSupported())
            return false;

        if (ExplicitInterfaceType is not null)
        {
            if (!ExplicitInterfaceType.IsSupported())
                return false;
        }

        if (_indexParameters is not null)
        {
            foreach (var parameter in _indexParameters)
            {
                if (!parameter.IsSupported())
                    return false;
            }
        }

        return true;
    }

    public string GetName(bool isInterfaceImplExplicitImplementation = false, List<TypeDef>? genericTypeParametersOverride = null)
    {
        string interfaceTypeName = "";
        TypeDef? interfaceType = null;

        if (ExplicitInterfaceType is not null)
        {
            interfaceType = ExplicitInterfaceType;
        }
        else if (isInterfaceImplExplicitImplementation)
        {
            interfaceType = _objectDef.Type;
        }

        if (interfaceType is not null)
        {
            if (genericTypeParametersOverride is not null)
            {
                interfaceType = interfaceType.OverrideGenericTypeParameter(genericTypeParametersOverride);
            }
            interfaceTypeName = $"{interfaceType.GetName()}.";
        }

        string name = IsIndexer ? "this" : _propertyName;
        return $"{interfaceTypeName}{name}";
    }

    public string GetOriginalName(bool isInterfaceImplExplicitImplementation = false)
    {
        string interfaceTypeName = "";
        if (ExplicitInterfaceType is not null)
        {
            interfaceTypeName = $"{ExplicitInterfaceType.GetOriginalName()}.";
        }
        else if (isInterfaceImplExplicitImplementation)
        {
            interfaceTypeName = $"{_objectDef.Type.GetOriginalName()}.";
        }

        return $"{interfaceTypeName}{_propertyName}";
    }

    public string GetNameOfExpression(bool isInterfaceImplExplicitImplementation = false)
    {
        if (ExplicitInterfaceType is not null || isInterfaceImplExplicitImplementation)
        {
            return $"\"{GetOriginalName(isInterfaceImplExplicitImplementation)}\"";
        }
        else
        {
            return $"nameof({GetName()})";
        }
    }

    public string GetSignatureId()
    {
        if (IsIndexer)
        {
            return $"{GetName()}[{ParameterDef.GetParametersSignatureId(_indexParameters!)}]";
        }
        else
        {
            return GetName();
        }
    }

    public string GetSignatureExpression()
    {
        string unsafeExpression = Type.IsUnsafe() ? "unsafe " : "";
        string accessorExpression = (_objectDef.Type.IsInterface || ExplicitInterfaceType is not null) ? "" : "public ";
        string staticExpression = _memberDefType == MemberDefType.Static ? "static " : "";
        string newExpression = (_hidesBase && ExplicitInterfaceType is null) ? "new " : "";
        string overrideExpression = _isOverride ? "override " : "";
        string abstractExpression = _isAbstract ? "abstract " : "";
        string virtualExpression = (_isVirtual && !_isOverride && !_isAbstract && ExplicitInterfaceType is null) ? "virtual " : "";
        string indexerNameExpression = (IsIndexer && ExplicitInterfaceType is null) ? $"[global::System.Runtime.CompilerServices.IndexerName(\"{_propertyName}\")]\n" : "";
        string indexerParametersExpression = IsIndexer ? $"[{ParameterDef.GetParametersSignatureExpression(_indexParameters!, genericTypeParametersOverride: null, isExtensionMethod: false)}]" : "";

        return $"{indexerNameExpression}{unsafeExpression}{accessorExpression}{staticExpression}{newExpression}{overrideExpression}{abstractExpression}{virtualExpression}{Type.GetTypeExpression()} {GetName()}{indexerParametersExpression}";
    }

    public string GetInterfaceImplSignatureExpression(bool isExplicitImplementation, List<TypeDef>? genericTypeParametersOverride)
    {
        string unsafeExpression = Type.IsUnsafe() ? "unsafe " : "";
        string accessorExpression = isExplicitImplementation ? "" : "public ";
        string staticExpression = _memberDefType == MemberDefType.Static ? "static " : "";
        string newExpression = "";
        string overrideExpression = "";
        string abstractExpression = "";
        string virtualExpression = "";
        string indexerParametersExpression = IsIndexer ? $"[{ParameterDef.GetParametersSignatureExpression(_indexParameters!, genericTypeParametersOverride, isExtensionMethod: false)}]" : "";

        TypeDef type = Type.OverrideGenericTypeParameter(genericTypeParametersOverride);
        return $"{unsafeExpression}{accessorExpression}{staticExpression}{newExpression}{overrideExpression}{abstractExpression}{virtualExpression}{type.GetTypeExpression()} {GetName(isExplicitImplementation, genericTypeParametersOverride)}{indexerParametersExpression}";
    }

    public string GetIndexerArgumentsExpression(List<TypeDef>? genericTypeParametersOverride)
    {
        if (!IsIndexer)
            return "";

        return ParameterDef.GetParametersArgumentExpression(_indexParameters!, genericTypeParametersOverride);
    }
}
