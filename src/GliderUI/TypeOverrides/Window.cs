using System.Management.Automation;
using GliderUI.Common;
using GliderUI.Generator;

namespace GliderUI.Avalonia.Controls;

public partial class Window : IGliderUIObject
{
    private const string _accessorClassName = "GliderUI.Server.WindowAccessor, GliderUI.Server";
    private readonly EventCallbackList _closedCallbacks = new();
    private bool _isShowCalled;
    private bool _isCloseCalled;
    private bool IsTerminated { get => _isShowCalled && (_isCloseCalled || IsClosed); }

    private int _isClosed = 1;
    internal bool IsClosed
    {
        get => Interlocked.CompareExchange(ref _isClosed, 0, 0) > 0;
        private set => Interlocked.Exchange(ref _isClosed, value ? 1 : 0);
    }

    [SurpressGeneratorMethodByName]
    public Window()
        : base(ObjectId.Null)
    {
        GliderUIObjectId = CommandClient.Get().CreateObject(
            ObjectTypeMapping.Get().GetTargetTypeName(typeof(Window)),
            this);

        CommandClient.Get().InvokeStaticMethod(_accessorClassName, "RegisterWindow", GliderUIObjectId);
    }

    internal Window(ObjectId id)
        : base(id)
    {
        CommandClient.Get().InvokeStaticMethod(_accessorClassName, "RegisterWindow", GliderUIObjectId);
    }

    [SurpressGeneratorMethodByName]
    public new void Show()
    {
        if (IsTerminated)
            return;

        _isShowCalled = true;
        IsClosed = false;
        CommandClient.Get().InvokeMethod(GliderUIObjectId, null, nameof(Show));
    }

    [SurpressGeneratorMethodByName]
    public new void AddClosed(ScriptBlock scriptBlock, object? argumentList = null)
    {
        AddClosed(new EventCallback
        {
            ScriptBlock = scriptBlock,
            ArgumentList = argumentList
        });
    }
    public new void AddClosed(EventCallback eventCallback)
    {
        _closedCallbacks.Add(
            GliderUIObjectId,
            "Closed",
            ObjectTypeMapping.Get().GetTargetTypeName(typeof(System.EventArgs)),
            eventCallback);
    }

    [SurpressGeneratorMethodByName]
    public void Close()
    {
        if (IsTerminated || IsClosed)
            return;

        _isCloseCalled = true;
        CommandClient.Get().InvokeMethod(GliderUIObjectId, null, nameof(Close));
    }

    public void WaitForClosed()
    {
        if (!_isShowCalled)
            return;

        while (true)
        {
            if (IsClosed && IsAllClosedCallbacksInvoked())
                return;

            Engine.Get().UpdateRunspace();
            Thread.Sleep(Constants.ClientCommandPolingIntervalMillisecond);
        }
    }

    private bool IsAllClosedCallbacksInvoked()
    {
        return _closedCallbacks.IsAllInvoked();
    }
}
