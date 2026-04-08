namespace GliderUI.Server;

internal interface IDisabledControlsHolder
{
    static abstract IDisabledControlsHolder Create(object?[]? controls);

    void Disable();
    void Enable();
}
