using GliderUI.Common;

namespace GliderUI.Avalonia.Controls;

public partial class Control
{
    private const string _accessorClassName = "GliderUI.Server.ControlAccessor, GliderUI.Server";

    public object? FindControl(string name)
    {
        return CommandClient.Get().InvokeStaticMethodAndGetResult<object>(
            _accessorClassName,
            "FindControl",
            GliderUIObjectId,
            name);
    }
}
