
namespace RpcUIShell.Core;

public class ObjectId
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";

    public static ObjectId Null { get; } = new();

    public ObjectId()
    {
    }

    public ObjectId(string id)
    {
        Id = id;
    }

    public bool IsNull()
    {
        return string.IsNullOrEmpty(Id);
    }

    public override string ToString()
    {
        return Id;
    }
}
