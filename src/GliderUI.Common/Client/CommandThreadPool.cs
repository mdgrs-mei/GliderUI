using System.Management.Automation;
using System.Management.Automation.Host;

namespace GliderUI.Common;

public class CommandThreadPool
{
    private PSHost? _streamingHost;
    private string _modulePath = "";
    private uint _defaultThreadCount;
    private CommandWorker[]? _workers;

    public CommandThreadPool()
    {
    }

    public void Init(
        PSHost? streamingHost,
        string modulePath,
        uint defaultThreadCount)
    {
        _streamingHost = streamingHost;
        _modulePath = modulePath;
        _defaultThreadCount = defaultThreadCount;
        Start(null, null, null);
    }

    public void Term()
    {
        Stop();
    }

    private void Start(
        uint? threadCount,
        string? initializationScript,
        object?[]? initializationScriptArgumentList)
    {
        if (_workers is not null)
            return;

        uint workerCount = threadCount ?? _defaultThreadCount;
        _workers = new CommandWorker[workerCount];
        for (int i = 0; i < _workers.Length; ++i)
        {
            var worker = new CommandWorker();
            worker.Start(
                _streamingHost,
                _modulePath,
                initializationScript,
                initializationScriptArgumentList);

            _workers[i] = worker;
        }
    }

    private void Stop()
    {
        if (_workers is null)
            return;

        foreach (var worker in _workers)
        {
            worker.Stop();
        }
        _workers = null;
    }

    public void SetOption(
        uint? threadCount,
        ScriptBlock? initializationScriptBlock,
        object?[]? initializationScriptBlockArgumentList)
    {
        string? initializationScript = initializationScriptBlock?.ToString();

        Stop();
        Start(
            threadCount,
            initializationScript,
            initializationScriptBlockArgumentList);
    }
}
