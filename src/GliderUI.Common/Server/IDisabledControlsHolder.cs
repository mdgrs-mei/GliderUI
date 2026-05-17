namespace RpcUIShell.Core;

public interface IDisabledControlsHolder
{
    static abstract IDisabledControlsHolder Create(object?[]? controls);

    void Disable();
    void Enable();
}
