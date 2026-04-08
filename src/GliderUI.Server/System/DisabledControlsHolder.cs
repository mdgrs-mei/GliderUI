using Avalonia.Input;

namespace GliderUI.Server;

internal sealed class DisabledControlsHolder : IDisabledControlsHolder
{
    private readonly List<InputElement>? _controls;

    public static IDisabledControlsHolder Create(object?[]? controls)
    {
        return new DisabledControlsHolder(controls);
    }

    private DisabledControlsHolder(object?[]? controls)
    {
        if (controls is null)
            return;

        _controls = [];
        foreach (var obj in controls)
        {
            if (obj is InputElement control)
            {
                if (control.IsEnabled)
                {
                    _controls.Add(control);
                }
            }
        }
    }

    public void Disable()
    {
        if (_controls is null)
            return;

        foreach (var control in _controls)
        {
            control.IsEnabled = false;
        }
    }

    public void Enable()
    {
        if (_controls is null)
            return;

        foreach (var control in _controls)
        {
            control.IsEnabled = true;
        }
    }
}
