using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GliderUI.ApiExporter;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            throw new ArgumentException("Specify a path to the output Api.xml file.");
        }

        string apiFilePath = args[0];

        var exporter = new Exporter(
            "GliderUI.ApiExporter",
            "GliderUI.Server");

        var api = exporter.Api;

        api.UnsupportedNamespaces =
        [
            "System.Linq.Expressions",
        ];

        api.UnsupportedTypes =
        [
            "System.IntPtr",
            "WinRT.IWinRTObject",
            "WinRT.IObjectReference",
            "WinRT.ObjectReference",
            "CompiledAvaloniaXaml.!AvaloniaResources",
            "CompiledAvaloniaXaml.!XamlLoader",
        ];

        api.SupportedGlobalSystemInterfaces =
        [
            "System.IDisposable",
            "System.Collections.Generic.ICollection",
            "System.Collections.Generic.IList",
            "System.Collections.IEnumerable",
            "System.Collections.Generic.IEnumerable",
            "System.Collections.IEnumerator",
            "System.Collections.Generic.IEnumerator",
            "System.Collections.Generic.IReadOnlyList",
            "System.Collections.Generic.IReadOnlyCollection",
        ];

        api.EmulatedSystemInterfaces =
        [
            "System.Collections.Generic.IDictionary",
            "System.Collections.IComparer",
            "System.Collections.IList",
            "System.Collections.ICollection",
        ];

        api.UnsupportedMethodNames =
        [
            "Equals",
            "GetHashCode",
            "GetType",
        ];

        exporter.AddTypesInAssembly(typeof(AvaloniaObject)); // Avalonia.Base.dll
        exporter.AddTypesInAssembly(typeof(Button)); // Avalonia.Controls.dll
        exporter.AddTypesInAssembly(typeof(AvaloniaRuntimeXamlLoader)); // Avalonia.Markup.Xaml.Loader.dll
        exporter.AddTypesInAssembly(typeof(DataGrid)); // Avalonia.Controls.DataGrid.dll
        exporter.AddTypesInAssembly(typeof(NativeWebView)); // Avalonia.Controls.WebView.dll

        exporter.AddObject(typeof(Server.DataSourcePropertyComparer));
        exporter.AddTypeMapping(typeof(Server.DataSource));

        exporter.Export(apiFilePath);
    }
}
