namespace GliderUI.Server;

#pragma warning disable CA1515 // Consider making public types internal
public enum EventCallbackRunspaceMode
#pragma warning restore CA1515 // Consider making public types internal
{
    MainRunspaceAsyncUI,
    MainRunspaceSyncUI,
    RunspacePoolAsyncUI,
}
