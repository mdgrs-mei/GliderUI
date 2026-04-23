namespace GliderUI.Common;

public interface IWindowStore
{
    object? EnterEventCallbackAndGetParentWindow(object sender);
    void ExitEventCallback(object? parentWindow);
}
