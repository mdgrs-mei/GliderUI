using System.Xml.Serialization;
using GliderUI.ApiExporter;
using Microsoft.CodeAnalysis;

namespace GliderUI.Generator;

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

            var api = LoadApi(apiText.ToString());
            if (api is null)
                return;

            var configOptionsProvider = providers.Left.Right;
            if (configOptionsProvider.GlobalOptions.TryGetValue("build_property.GliderUIGenerator_GenerateTypeMapping", out var generateTypeMapping))
            {
                EnumGenerator.GenerateTypeMapping(sourceProductionContext, api);
                ObjectGenerator.GenerateTypeMapping(sourceProductionContext, api);
            }
            else
            {
                var surpressMethodByNameAttributes = providers.Right.Left;
                var surpressPropertyByNameAttributes = providers.Right.Right;
                AttributeGenerator.InitSurpressDictionary(
                    surpressMethodByNameAttributes,
                    surpressPropertyByNameAttributes);

                EnumGenerator.Generate(sourceProductionContext, api);
                ObjectGenerator.Generate(sourceProductionContext, api);

                AttributeGenerator.TermSurpressDictionary();
            }
        });
    }

    private static Api LoadApi(string content)
    {
        var stringReader = new StringReader(content);
        var serializer = new XmlSerializer(typeof(Api));
        var api = (Api)serializer.Deserialize(stringReader);
        return api;
    }

    internal static string GetTargetNamespace(string serverNamespace)
    {
        if (serverNamespace is "GliderUI.Server" or "GliderUI.Common")
        {
            return "GliderUI";
        }
        else
        {
            return $"GliderUI.{serverNamespace}";
        }
    }
}
