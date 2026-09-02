using System.ComponentModel.DataAnnotations;

namespace MessagesProcessor.Messages;

public abstract class BaseData
{
    [Range(1, int.MaxValue, ErrorMessage = "OrderId must be a positive value.")]
    public int OrderId { get; set; }
}
