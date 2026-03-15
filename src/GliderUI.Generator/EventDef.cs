using GliderUI.ApiExporter;

namespace GliderUI.Generator;

internal class EventDef
{
    private readonly Api.EventDef _apiEventDef;
    private readonly MemberDefType _memberDefType;

    public ObjectDef ObjectDef { get; }
    public List<ParameterDef> Parameters { get; } = [];
    public TypeDef? ExplicitInterfaceType;

    public EventDef(
        Api.EventDef apiEventDef,
        ObjectDef objectDef,
        MemberDefType memberDefType)
    {
        _apiEventDef = apiEventDef;
        ObjectDef = objectDef;
        _memberDefType = memberDefType;

        if (_apiEventDef.Parameters is not null)
        {
            foreach (var apiParameterDef in _apiEventDef.Parameters)
            {
                var parameter = new ParameterDef(apiParameterDef, useSystemInterfaceName: false);
                Parameters.Add(parameter);
            }
        }

        ExplicitInterfaceType = apiEventDef.ExplicitInterfaceType is null ? null : new TypeDef(apiEventDef.ExplicitInterfaceType);
    }

    public bool IsSupported()
    {
        // Only support (sender, eventArgs) pattern.
        if (Parameters.Count != 2)
            return false;

        foreach (var parameter in Parameters)
        {
            if (!parameter.IsSupported())
                return false;
        }

        if (ExplicitInterfaceType is not null)
        {
            if (!ExplicitInterfaceType.IsSupported())
                return false;
        }

        return true;
    }

    public static string GetEventCallbackListExpression(MemberDefType memberDefType)
    {
        string staticExpression = memberDefType == MemberDefType.Static ? "static " : "";
        return $"private {staticExpression}readonly EventCallbackList {GetEventCallbackListFieldName(memberDefType)} = new();";
    }

    private static string GetEventCallbackListFieldName(MemberDefType memberDefType)
    {
        string prefix = memberDefType == MemberDefType.Static ? "s_" : "_";
        return $"{prefix}callbacks";
    }

    public string GetMethodFullName(bool isInterfaceImplExplicitImplementation = false)
    {
        string interfaceTypeName = "";

        if (ExplicitInterfaceType is not null)
        {
            interfaceTypeName = $"{ExplicitInterfaceType.GetName()}.";
        }
        else if (isInterfaceImplExplicitImplementation)
        {
            interfaceTypeName = $"{ObjectDef.Type.GetGlobalSystemInterfaceName()}.";
        }

        return $"{interfaceTypeName}{GetMethodName()}";
    }

    private string GetMethodName()
    {
        return $"Add{GetEventName()}";
    }

    private string GetEventName()
    {
        return _apiEventDef.Name;
    }

    public string GetSignatureId()
    {
        return $"{GetMethodName()}({GetParametersSignatureId()})";
    }

    public string GetScriptBlockMethodSignatureExpression()
    {
        string accessorExpression = (ObjectDef.Type.IsInterface || ExplicitInterfaceType is not null) ? "" : "public ";
        return GetScriptBlockMethodSignatureExpression(
            accessorExpression,
            isInterfaceImplExplicitImplementation: false);
    }

    private string GetScriptBlockMethodSignatureExpression(string accessorExpression, bool isInterfaceImplExplicitImplementation)
    {
        string staticExpression = _memberDefType == MemberDefType.Static ? "static " : "";
        string defaultValueExpression = ExplicitInterfaceType is null ? " = null" : "";

        return $"{accessorExpression}{staticExpression}void {GetMethodFullName(isInterfaceImplExplicitImplementation)}(ScriptBlock scriptBlock, object? argumentList{defaultValueExpression})";
    }

    public string GetScriptBlockMethodExpression()
    {
        string accessorExpression = (ObjectDef.Type.IsInterface || ExplicitInterfaceType is not null) ? "" : "public ";
        return GetScriptBlockMethodExpression(accessorExpression, isInterfaceImplExplicitImplementation: false);
    }

    public string GetInterfaceImplScriptBlockMethodExpression(bool isExplicitImplementation)
    {
        string accessorExpression = isExplicitImplementation ? "" : "public ";
        return GetScriptBlockMethodExpression(accessorExpression, isExplicitImplementation);
    }

    private string GetScriptBlockMethodExpression(string accessorExpression, bool isInterfaceImplExplicitImplementation)
    {
        return $$"""
            {{GetScriptBlockMethodSignatureExpression(accessorExpression, isInterfaceImplExplicitImplementation)}}
            {
                {{GetMethodName()}}(new EventCallback
                {
                    ScriptBlock = scriptBlock,
                    ArgumentList = argumentList
                });
            }
            """;
    }

    public string GetEventCallbackMethodSignatureExpression()
    {
        string accessorExpression = (ObjectDef.Type.IsInterface || ExplicitInterfaceType is not null) ? "" : "public ";
        return GetEventCallbackMethodSignatureExpression(
            accessorExpression,
            isInterfaceImplExplicitImplementation: false);
    }

    private string GetEventCallbackMethodSignatureExpression(string accessorExpression, bool isInterfaceImplExplicitImplementation)
    {
        string staticExpression = _memberDefType == MemberDefType.Static ? "static " : "";
        return $"{accessorExpression}{staticExpression}void {GetMethodFullName(isInterfaceImplExplicitImplementation)}(EventCallback eventCallback)";
    }

    public string GetEventCallbackMethodExpression()
    {
        string accessorExpression = (ObjectDef.Type.IsInterface || ExplicitInterfaceType is not null) ? "" : "public ";
        return GetEventCallbackMethodExpression(
            ObjectDef.Type.GetName(),
            accessorExpression,
            isInterfaceImplExplicitImplementation: false,
            genericTypeParametersOverride: null);
    }

    public string GetInterfaceImplEventCallbackMethodExpression(
        string rootClassName,
        bool isExplicitImplementation,
        List<TypeDef>? genericTypeParametersOverride)
    {
        string accessorExpression = isExplicitImplementation ? "" : "public ";
        return GetEventCallbackMethodExpression(
            rootClassName,
            accessorExpression,
            isExplicitImplementation,
            genericTypeParametersOverride);
    }

    private string GetEventCallbackMethodExpression(
        string className,
        string accessorExpression,
        bool isInterfaceImplExplicitImplementation,
        List<TypeDef>? genericTypeParametersOverride)
    {
        string signatureExpression = GetEventCallbackMethodSignatureExpression(accessorExpression, isInterfaceImplExplicitImplementation);
        if (_memberDefType == MemberDefType.Static)
        {
            return $$"""
                {{signatureExpression}}
                {
                    {{GetEventCallbackListFieldName(_memberDefType)}}.AddStatic(
                        ObjectTypeMapping.Get().GetTargetTypeName(typeof({{className}})),
                        "{{GetEventName()}}",
                        ObjectTypeMapping.Get().GetTargetTypeName(typeof({{GetEventArgsTypeName(genericTypeParametersOverride)}})),
                        eventCallback);
                }
                """;
        }
        else
        {
            return $$"""
                {{signatureExpression}}
                {
                    {{GetEventCallbackListFieldName(_memberDefType)}}.Add(
                        GliderUIObjectId,
                        "{{GetEventName()}}",
                        ObjectTypeMapping.Get().GetTargetTypeName(typeof({{GetEventArgsTypeName(genericTypeParametersOverride)}})),
                        eventCallback);
                }
                """;
        }
    }

    private string GetEventArgsTypeName(List<TypeDef>? genericTypeParametersOverride)
    {
        var type = Parameters[1].Type.OverrideGenericTypeParameter(genericTypeParametersOverride);
        return type.GetName();
    }

    private string GetParametersSignatureId()
    {
        return ParameterDef.GetParametersSignatureId(Parameters);
    }
}
