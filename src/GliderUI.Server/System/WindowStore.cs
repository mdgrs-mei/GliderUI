using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using GliderUI.Common;

namespace GliderUI.Server;

internal sealed class WindowStore : Singleton<WindowStore>
{
    internal sealed class WindowProperty
    {
        public int RunningEventCallbackCount { get; set; }
    }

    private sealed class Comparer : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y)
        {
            return ReferenceEquals(x, y);
        }
        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    private readonly Dictionary<Window, WindowProperty> _windowProperties = new(new Comparer());

    public WindowStore()
    {
    }

    public void RegisterWindow(Window window)
    {
        _windowProperties[window] = new();
    }

    public WindowProperty GetWindowProperty(Window window)
    {
        if (_windowProperties.TryGetValue(window, out var property))
        {
            return property;
        }
        else
        {
            throw new InvalidOperationException($"WindowStore: Window not found [{window}].");
        }
    }

    public Window? EnterEventCallbackAndGetParentWindow(object sender)
    {
        Window? parentWindow = GetParentWindow(sender);
        if (parentWindow is null)
            return null;

        var property = _windowProperties[parentWindow];
        lock (property)
        {
            property.RunningEventCallbackCount++;
        }
        return parentWindow;
    }

    public void ExitEventCallback(Window? parentWindow)
    {
        if (parentWindow is null)
            return;

        var property = GetWindowProperty(parentWindow);
        lock (property)
        {
            property.RunningEventCallbackCount--;
            Monitor.Pulse(property);
        }
    }

    public Window? GetParentWindow(object sender)
    {
        if (sender is Window w)
            return w;

        if (sender is StyledElement element)
        {
            while (element.Parent is not null)
            {
                if (element.Parent is Window parentWindow)
                    return parentWindow;
                element = element.Parent;
            }
        }
        return null;
    }

    public async Task WaitForAllChildEventCallbacksFinishedAsync(Window window)
    {
        var property = GetWindowProperty(window);

        await Task.Run(() =>
        {
            lock (property)
            {
                while (property.RunningEventCallbackCount > 0)
                {
                    _ = Monitor.Wait(property);
                }
            }
        });
    }
}
