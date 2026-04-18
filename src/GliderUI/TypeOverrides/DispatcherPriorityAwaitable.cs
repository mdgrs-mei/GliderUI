using GliderUI.Generator;

namespace GliderUI.Avalonia.Threading;

public partial class DispatcherPriorityAwaitable<T>
{
    [SurpressGeneratorMethodByName]
    public void WaitForCompleted()
    {
        var awaiter = GetAwaiter();
        while (!awaiter.IsCompleted)
        {
            Engine.Get().UpdateRunspace();
            Thread.Sleep(Constants.ClientCommandPolingIntervalMillisecond);
        }
    }
}
