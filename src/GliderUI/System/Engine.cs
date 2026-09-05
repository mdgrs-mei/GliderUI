using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using RpcUIShell.Core;

namespace GliderUI;

public class Engine
{
    private sealed class RunspaceState
    {
        public bool IsInitialized { get; set; }
        public bool IsMain { get; set; }
        public bool IsInUpdate { get; set; }
        public global::System.Timers.Timer? EventTimer;
        public PSEventSubscriber? TimerEventSubscriber;

        public void InitTimerEvent()
        {
            // Register timer event to process the main command queue.
            // The timer event fires when commands are processed on the main runspace or when waiting for user inputs in interactive sessions.
            EventTimer = new()
            {
                Interval = Constants.ClientTimerEventCommandPolingIntervalMillisecond,
                AutoReset = false,
                Enabled = false
            };

            ScriptBlock action = ScriptBlock.Create(@"
[GliderUI.Engine]::Get().IdleUpdateRunspace()
$engineUpdateTimer = $Sender
$engineUpdateTimer.Start()
"
            );

            TimerEventSubscriber = Runspace.DefaultRunspace.Events.SubscribeEvent(
                source: EventTimer,
                eventName: "Elapsed",
                sourceIdentifier: "",
                data: null,
                action: action,
                supportEvent: false,
                forwardEvent: false);

            EventTimer.Start();
        }

        public void TermTimerEvent()
        {
            if (EventTimer is null)
                return;

            EventTimer.Stop();
            Runspace.DefaultRunspace.Events.UnsubscribeEvent(TimerEventSubscriber);
        }
    }

    private readonly RunspaceLocal<RunspaceState> _thisRunspace = new(() => new RunspaceState());
    private int _remainingMainRunspaceCount = 1;
    private string _upstreamPipeName = "";
    private string _downstreamPipeName = "";
    private Process? _serverProcess;
    private readonly CommandThreadPool _commandThreadPool = new();

    private static readonly Engine _instance = new();
    public static Engine Get()
    {
        return _instance;
    }

    public bool AcquireMainRunspace()
    {
        return Interlocked.Exchange(ref _remainingMainRunspaceCount, 0) > 0;
    }

    public void InitMainRunspace(
        string serverExePath,
        PSHost? streamingHost,
        string modulePath,
        bool useTimerEvent)
    {
        var thisRunspace = _thisRunspace.Value;
        if (thisRunspace.IsInitialized)
            return;

#if DEBUG
        //System.Diagnostics.Debugger.Launch();
#endif
        InitPipeNames();
        try
        {
            StartServerProcess(serverExePath);
            InitConnection();
        }
        catch (Exception)
        {
            StopServerProcess();
            Console.Error.WriteLine($"Failed to start server [{serverExePath}]");
            throw;
        }
        InitCommandThreadPool(streamingHost, modulePath);

        if (useTimerEvent)
        {
            thisRunspace.InitTimerEvent();
        }

        thisRunspace.IsInitialized = true;
        thisRunspace.IsMain = true;
    }

    public void InitSubRunspace(bool useTimerEvent)
    {
        var thisRunspace = _thisRunspace.Value;
        if (thisRunspace.IsInitialized)
            return;

        if (useTimerEvent)
        {
            thisRunspace.InitTimerEvent();
        }

        thisRunspace.IsInitialized = true;
    }

    public void TermRunspace()
    {
        var thisRunspace = _thisRunspace.Value;
        if (!thisRunspace.IsInitialized)
            return;

        thisRunspace.IsInitialized = false;

        thisRunspace.TermTimerEvent();

        if (thisRunspace.IsMain)
        {
            TermCommandThreadPool();
            TermConnection();
            StopServerProcess();
            thisRunspace.IsMain = false;
        }
    }

    private void InitPipeNames()
    {
        var processId = Environment.ProcessId.ToString();
        _upstreamPipeName = $"GliderUI.ClientToServer.{processId}";
        _downstreamPipeName = $"GliderUI.ServerToClient.{processId}";
    }

    private void StartServerProcess(string path)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = path
        };
        startInfo.ArgumentList.Add(_upstreamPipeName);
        startInfo.ArgumentList.Add(_downstreamPipeName);
        startInfo.ArgumentList.Add(Environment.ProcessId.ToString());
        _serverProcess = Process.Start(startInfo);
    }

    private void StopServerProcess()
    {
        if (_serverProcess is null)
            return;

        _serverProcess.Kill();
        _serverProcess = null;
    }

    private void InitConnection()
    {
        ObjectStore.Get().SetObjectIdPrefix("c");
        TypeMappingInitializer.Init();
        RpcValueConverter.Get().DefaultObjectType = typeof(GliderUIObject);
        CommandServer.Get().Init(_downstreamPipeName);
        CommandClient.Get().Init(_upstreamPipeName);
    }

    private void TermConnection()
    {
        CommandClient.Get().Term();
        CommandServer.Get().Term();
    }

    private void InitCommandThreadPool(PSHost? streamingHost, string modulePath)
    {
        _commandThreadPool.Init(streamingHost, modulePath, Constants.ClientCommandThreadPoolDefaultThreadCount);
    }

    private void TermCommandThreadPool()
    {
        _commandThreadPool.Term();
    }

    public void SetCommandThreadPoolOption(
        uint? threadCount,
        ScriptBlock? initializationScriptBlock,
        object?[]? initializationScriptBlockArgumentList)
    {
        _commandThreadPool.SetOption(
            threadCount,
            initializationScriptBlock,
            initializationScriptBlockArgumentList);
    }

    public void IdleUpdateRunspace()
    {
        var thisRunspace = _thisRunspace.Value;
        if (!thisRunspace.IsInitialized)
            return;

        // Do not run commands inside other event callbacks.
        if (thisRunspace.IsInUpdate)
            return;

        ProcessCommands();
    }

    internal void UpdateRunspace()
    {
        var thisRunspace = _thisRunspace.Value;
        if (!thisRunspace.IsInitialized)
            return;

        if (!thisRunspace.IsInUpdate)
        {
            // Root update.
            thisRunspace.IsInUpdate = true;
            ProcessCommands();
            thisRunspace.IsInUpdate = false;
        }
        else
        {
            // Recursive update.
            ProcessCommands();
        }
    }

    private void ProcessCommands()
    {
        var queueId = new CommandQueueId(CommandQueueType.RunspaceId, Runspace.DefaultRunspace.Id);
        try
        {
            CommandServer.Get().ProcessCommands(queueId);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Engine.ProcessCommands faild:");
            Console.Error.WriteLine($"{e.GetType().FullName}: {e.Message}");
            if (e.InnerException is not null)
            {
                Console.Error.WriteLine($"-> {e.InnerException.GetType().FullName}: {e.InnerException.Message}");
            }
        }
    }
}
