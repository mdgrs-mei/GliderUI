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
