using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using RpcUIShell.Core;

namespace RpcUIShell.Generator;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(AttributeGenerator.Generate);

        var additionalTextsProvider = context.AdditionalTextsProvider.Where((text) =>
        {
            return text.Path.EndsWith("Api.xml");
        }).Collect();

        var methodAttributesProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeGenerator.SurpressMethodByNameAttributeFullName,
            (syntaxNode, cancellationToken) => true,
            (generatorAttributeSyntaxContext, cancellationToken) => generatorAttributeSyntaxContext).Collect();

        var prropertyAttributesProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeGenerator.SurpressPropertyByNameAttributeFullName,
            (syntaxNode, cancellationToken) => true,
            (generatorAttributeSyntaxContext, cancellationToken) => generatorAttributeSyntaxContext).Collect();

        var attributesProvider = methodAttributesProvider.Combine(prropertyAttributesProvider);

        var provider = additionalTextsProvider.Combine(context.AnalyzerConfigOptionsProvider).Combine(attributesProvider);

        context.RegisterSourceOutput(provider, (sourceProductionContext, providers) =>
        {
            var apiText = providers.Left.Left.FirstOrDefault()?.GetText();
            if (apiText is null)
                return;

            LoadApi(apiText.ToString());

            var configOptionsProvider = providers.Left.Right;
            if (configOptionsProvider.GlobalOptions.TryGetValue("build_property.RpcUIShellGenerator_GenerateTypeMapping", out var rootNamespace))
            {
                EnumGenerator.GenerateTypeMapping(sourceProductionContext, rootNamespace);
                ObjectGenerator.GenerateTypeMapping(sourceProductionContext, rootNamespace);
            }

            if (configOptionsProvider.GlobalOptions.TryGetValue("build_property.RpcUIShellGenerator_GenerateApi", out var generateApi))
            {
                var surpressMethodByNameAttributes = providers.Right.Left;
                var surpressPropertyByNameAttributes = providers.Right.Right;
                AttributeGenerator.InitSurpressDictionary(
                    surpressMethodByNameAttributes,
                    surpressPropertyByNameAttributes);

                EnumGenerator.Generate(sourceProductionContext);
                ObjectGenerator.Generate(sourceProductionContext);

                AttributeGenerator.TermSurpressDictionary();
            }

            UnloadApi();
        });
    }

    public static Api? Api { get; private set; }

    private static void LoadApi(string content)
    {
        var stringReader = new StringReader(content);
        var serializer = new XmlSerializer(typeof(Api));
        Api = (Api)serializer.Deserialize(stringReader);
    }

    private static void UnloadApi()
    {
        Api = null;
    }

    internal static string GetTargetNamespace(string serverNamespace)
    {
        if (serverNamespace is "GliderUI.Server" or "RpcUIShell.Core")
        {
            return "GliderUI";
        }
        else
        {
            return $"GliderUI.{serverNamespace}";
        }
    }
}
