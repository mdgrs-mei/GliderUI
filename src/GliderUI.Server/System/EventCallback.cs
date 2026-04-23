using GliderUI.Common;

namespace GliderUI.Server;

internal static class EventCallback
{
    static EventCallback()
    {
        EventCallbackBinder<DisabledControlsHolder>.DefaultEventArgsTypeName = "GliderUI.GliderUIObject, GliderUI";
        EventCallbackBinder<DisabledControlsHolder>.WindowStore = WindowStore.Get();

        EventCallbackBinder<DisabledControlsHolder>.BlockingWaitTask = (task) =>
        {
            while (!task.IsCompleted)
            {
                App.ProcessCommands();
                Thread.Sleep(Constants.ServerSyncUICommandPolingIntervalMillisecond);
            }
            App.ProcessCommands();
        };
    }

    public static void Add(
        object target,
        string eventName,
        string eventArgsTypeName,
        EventCallbackRunspaceMode runspaceMode,
        int mainRunspaceId,
        string eventListId,
        int eventId,
        object?[]? disabledControlsWhileProcessing)
    {
        var targetType = target.GetType();

        EventCallbackBinder<DisabledControlsHolder>.Add(
            target,
            targetType,
            eventName,
            eventArgsTypeName,
            runspaceMode,
            mainRunspaceId,
            eventListId,
            eventId,
            disabledControlsWhileProcessing);
    }

    public static void AddStatic(
        string className,
        string eventName,
        string eventArgsTypeName,
        EventCallbackRunspaceMode runspaceMode,
        int mainRunspaceId,
        string eventListId,
        int eventId,
        object?[]? disabledControlsWhileProcessing)
    {
        var targetType = Type.GetType(className);
        if (targetType is null)
        {
            throw new InvalidOperationException($"Type [{className}] not found.");
        }

        EventCallbackBinder<DisabledControlsHolder>.Add(
            null,
            targetType,
            eventName,
            eventArgsTypeName,
            runspaceMode,
            mainRunspaceId,
            eventListId,
            eventId,
            disabledControlsWhileProcessing);
    }
}
