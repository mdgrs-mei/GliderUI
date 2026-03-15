using Avalonia.Controls;

namespace GliderUI.Server;

internal static class ControlAccessor
{
    public static object? FindControl(
        Control control,
        string name)
    {
        return control.Find<object>(name);
    }
}
