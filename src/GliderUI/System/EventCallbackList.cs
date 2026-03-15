using System.Management.Automation.Runspaces;
using GliderUI.Common;

namespace GliderUI;

internal sealed class EventCallbackList : IGliderUIObject
{
    private readonly List<EventCallback> _callbacks = [];
    public ObjectId GliderUIObjectId { get; } = new();

    public EventCallbackList()
    {
        _ = ObjectStore.Get().RegisterObject(this, out ObjectId id);
        GliderUIObjectId = id;
    }

    public void Add(
        ObjectId targetObjectId,
        string eventName,
        string eventArgsTypeName,
        EventCallback? eventCallback)
    {
        if (eventCallback is null)
            return;

        var copiedEventCallback = eventCallback.Copy();

        int eventId = 0;
        lock (_callbacks)
        {
            eventId = _callbacks.Count;
            _callbacks.Add(copiedEventCallback);
        }

        ObjectId[]? disabledControlIds = copiedEventCallback.GetDisabledControlIds();

        CommandClient.Get().InvokeStaticMethod(
            "GliderUI.Server.EventCallback, GliderUI.Server",
            "Add",
            targetObjectId,
            eventName,
            eventArgsTypeName,
            copiedEventCallback.RunspaceMode,
            Runspace.DefaultRunspace.Id,
            GliderUIObjectId.Id,
            eventId,
            disabledControlIds);
    }

    public void AddStatic(
        string className,
        string eventName,
        string eventArgsTypeName,
        EventCallback? eventCallback)
    {
        if (eventCallback is null)
            return;

        var copiedEventCallback = eventCallback.Copy();

        int eventId = 0;
        lock (_callbacks)
        {
            eventId = _callbacks.Count;
            _callbacks.Add(copiedEventCallback);
        }

        ObjectId[]? disabledControlIds = copiedEventCallback.GetDisabledControlIds();

        CommandClient.Get().InvokeStaticMethod(
            "GliderUI.Server.EventCallback, GliderUI.Server",
            "AddStatic",
            className,
            eventName,
            eventArgsTypeName,
            copiedEventCallback.RunspaceMode,
            Runspace.DefaultRunspace.Id,
            GliderUIObjectId.Id,
            eventId,
            disabledControlIds);
    }

    public void Invoke(int eventId, object? sender, object? eventArgs)
    {
        EventCallback? callback = null;
        lock (_callbacks)
        {
            if (eventId < 0 || eventId >= _callbacks.Count)
            {
                return;
            }
            callback = _callbacks[eventId];
        }
        callback.Invoke(sender, eventArgs);
    }

    public bool IsAllInvoked()
    {
        lock (_callbacks)
        {
            foreach (var callback in _callbacks)
            {
                if (!callback.IsInvoked)
                    return false;
            }
            return true;
        }
    }
}
