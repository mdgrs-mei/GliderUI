using RpcUIShell.Core;

namespace GliderUI;

public sealed class GliderUIObject : IGliderUIObject
{
    public ObjectId GliderUIObjectId { get; } = new();

    internal GliderUIObject(ObjectId id)
    {
        GliderUIObjectId = id;
    }
}
