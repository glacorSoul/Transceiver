using MessagePack;

namespace Transceiver.Serializer.SerializerMessagePack;

public class ServerResponseMessagePack<TResponse> : IServerResponse<TResponse>
{
    public ServerResponseMessagePack() : this(default!)
    {
    }

    public ServerResponseMessagePack(TResponse data) : this(data, Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
    }

    public ServerResponseMessagePack(TResponse data, Guid id) : this(data, id, DateTimeOffset.UtcNow)
    {
    }

    public ServerResponseMessagePack(TResponse data, Guid id, DateTimeOffset timeStamp)
    {
        Data = data;
        Id = id;
        TimeStamp = timeStamp;
    }

    [Key(0)]
    public DateTimeOffset TimeStamp { get; set; }
    [Key(1)]
    public TResponse Data { get; set; }
    [Key(2)]
    public Guid Id { get; }
}
