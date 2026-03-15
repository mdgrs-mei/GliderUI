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
        var exporter = new Exporter();
        exporter.Export(apiFilePath);
    }
}
