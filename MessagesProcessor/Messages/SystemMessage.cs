namespace MessagesProcessor.Messages;

public class SystemMessage<T> where T : BaseData, new()
{
    public required string DataType { get; set; }

    public required T Data { get; set; }
}
