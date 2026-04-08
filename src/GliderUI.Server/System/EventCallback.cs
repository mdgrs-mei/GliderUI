namespace GliderUI.Server;

internal static class EventCallback
{
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

        EventCallbackBase<DisabledControlsHolder>.Add(
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

        EventCallbackBase<DisabledControlsHolder>.Add(
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
