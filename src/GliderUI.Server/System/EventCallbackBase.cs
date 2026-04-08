using System.Diagnostics;
using System.Reflection;
using GliderUI.Common;

namespace GliderUI.Server;

internal static class EventCallbackBase<TDisabledControlsHolder> where TDisabledControlsHolder : IDisabledControlsHolder
{
    private static readonly MethodInfo s_callbackCreatorGeneric = typeof(EventCallbackBase<TDisabledControlsHolder>).GetMethod("Create", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;

    public static void Add(
        object? target,
        Type targetType,
        string eventName,
        string eventArgsTypeName,
        EventCallbackRunspaceMode runspaceMode,
        int mainRunspaceId,
        string eventListId,
        int eventId,
        object?[]? disabledControlsWhileProcessing)
    {
        var eventInfo = targetType.GetEvent(eventName);
        if (eventInfo is null)
        {
            throw new InvalidOperationException($"Event [{eventName}] not found in [{targetType.Name}].");
        }

        var eventArgsType = Type.GetType(eventArgsTypeName);
        if (eventArgsType is null)
        {
            throw new InvalidOperationException($"Type [{eventArgsTypeName}] not found.");
        }

        var callbackCreator = s_callbackCreatorGeneric.MakeGenericMethod(eventArgsType);

        var callback = callbackCreator.Invoke(null, [
            runspaceMode,
            mainRunspaceId,
            eventListId,
            eventId,
            disabledControlsWhileProcessing])!;

        var callbackType = callback.GetType();
        var callbackTargetProperty = callbackType.GetProperty("Target", BindingFlags.Instance | BindingFlags.Public)!;
        var callbackTarget = callbackTargetProperty.GetValue(callback)!;
        var callbackMethodInfoProperty = callbackType.GetProperty("Method", BindingFlags.Instance | BindingFlags.Public)!;
        var callbackMethodInfo = (MethodInfo)callbackMethodInfoProperty.GetValue(callback)!;

        var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType!, callbackTarget, callbackMethodInfo);
        eventInfo.AddEventHandler(target, handler);
    }

    internal static Action<object, TEventArgs> Create<TEventArgs>(
        EventCallbackRunspaceMode runspaceMode,
        int mainRunspaceId,
        string eventListId,
        int eventId,
        object?[]? disabledControlsWhileProcessing)
    {
        return async (object sender, TEventArgs eventArgs) =>
        {
            var parentWindow = WindowStore.Get().EnterEventCallbackAndGetParentWindow(sender);

            IDisabledControlsHolder disabledControls = TDisabledControlsHolder.Create(disabledControlsWhileProcessing);
            disabledControls.Disable();

            var senderId = ObjectStore.Get().GetId(sender);
            var temporaryQueueId = CommandClient.Get().CreateTemporaryQueueId();
            var processingQueueId = GetProcessingQueueId(runspaceMode, mainRunspaceId);

            Type eventArgsType = typeof(TEventArgs);
            var eventArgsTypeName = (eventArgsType == typeof(object)) ? "GliderUIObject" : eventArgsType.ToString();
            var eventArgsId = CommandClient.Get().CreateObjectWithId(
                temporaryQueueId,
                $"GliderUI.{eventArgsTypeName}, GliderUI",
                eventArgs);

            var invokeTask = CommandClient.Get().InvokeMethodWaitAsync(
                temporaryQueueId,
                new ObjectId(eventListId),
                null,
                "Invoke",
                eventId,
                senderId,
                eventArgsId);

            CommandClient.Get().ProcessTemporaryQueue(processingQueueId, temporaryQueueId);

            try
            {
                if (runspaceMode == EventCallbackRunspaceMode.MainRunspaceSyncUI)
                {
                    BlockingWaitTask(invokeTask);
                }
                else
                {
                    await invokeTask;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("EventCallback faild:");
                Debug.WriteLine(e);
                CommandClient.Get().WriteError("EventCallback faild:");
                CommandClient.Get().WriteException(e);
            }

            CommandClient.Get().DestroyObject(processingQueueId, eventArgsId);
            disabledControls.Enable();

            WindowStore.Get().ExitEventCallback(parentWindow);
        };
    }

    private static CommandQueueId GetProcessingQueueId(EventCallbackRunspaceMode runspaceMode, int mainRunspaceId)
    {
        if (runspaceMode == EventCallbackRunspaceMode.RunspacePoolAsyncUI)
        {
            return CommandQueueId.ThreadPool;
        }
        else
        {
            return new CommandQueueId(CommandQueueType.RunspaceId, mainRunspaceId);
        }
    }

    private static void BlockingWaitTask(Task task)
    {
        while (!task.IsCompleted)
        {
            App.ProcessCommands();
            Thread.Sleep(Constants.ServerSyncUICommandPolingIntervalMillisecond);
        }
        App.ProcessCommands();
    }
}
