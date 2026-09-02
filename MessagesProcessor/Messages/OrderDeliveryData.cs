using System.ComponentModel.DataAnnotations;

namespace MessagesProcessor.Messages;

public class OrderDeliveryData : BaseData
{
    [Required(ErrorMessage = "CustomerName is required.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "CustomerAddress is required.")]
    public string CustomerAddress { get; set; } = string.Empty;

    [Range(1, long.MaxValue, ErrorMessage = "CustomerPhoneNumber is required.")]
    public long CustomerPhoneNumber { get; set; }
}
