using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GliderUI.Common;

namespace GliderUI.Server;

internal sealed partial class App : Application
{
    private Process? _parentProcess;
    private string _upstreamPipeName = "";
    private string _downstreamPipeName = "";
    private DispatcherTimer? _updateTimer;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Init();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;
        }

        _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(Constants.ServerCommandPolingIntervalMillisecond)
        };
        _updateTimer.Tick += (sender, eventArgs) =>
        {
            var continueUpdate = Update();
            if (!continueUpdate)
            {
                _updateTimer.Stop();
            }
        };
        _updateTimer.Start();

        base.OnFrameworkInitializationCompleted();
    }

    private void Init()
    {
        ParseArgs();
        ObjectStore.Get().SetObjectIdPrefix("s");
        ObjectTypeMapping.Get().Direction = ObjectTypeMapping.MappingDirection.ServerToClient;
        CommandServer.Get().Init(_upstreamPipeName);
        CommandClient.Get().Init(_downstreamPipeName);
        ObjectValidator.Init();

        BindingPlugins.PropertyAccessors.Add(new DataSourcePropertyAccessorPlugin());
    }

    private void Term()
    {
        ObjectValidator.Term();
        CommandClient.Get().Term();
        CommandServer.Get().Term();
    }

    private void ParseArgs()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        if (arguments.Length != 4)
        {
            throw new ArgumentException($"Invalid arguments {arguments}");
        }
        _upstreamPipeName = arguments[1];
        _downstreamPipeName = arguments[2];

        var parentProcessId = int.Parse(arguments[3]);
        _parentProcess = Process.GetProcessById(parentProcessId);
    }

    private bool Update()
    {
        if (ParentProcessExited())
        {
            Term();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
            return false;
        }

        ProcessCommands();
        return true;
    }

    private bool ParentProcessExited()
    {
        if (_parentProcess is null)
        {
            return true;
        }
        return _parentProcess.HasExited;
    }

    public static void ProcessCommands()
    {
        try
        {
            CommandServer.Get().ProcessCommands(CommandQueueId.MainThread);
        }
        catch (Exception e)
        {
            Debug.WriteLine("App.ProcessCommands faild:");
            Debug.WriteLine(e);
            CommandClient.Get().WriteError("App.ProcessCommands faild:");
            CommandClient.Get().WriteException(e);
        }
    }
}
