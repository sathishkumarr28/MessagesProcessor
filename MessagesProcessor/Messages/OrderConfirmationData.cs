using System.ComponentModel.DataAnnotations;

namespace MessagesProcessor.Messages;

public class OrderConfirmationData : BaseData
{
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "CustomerName is required.")]
    public string CustomerName { get; set; } = string.Empty;
}
