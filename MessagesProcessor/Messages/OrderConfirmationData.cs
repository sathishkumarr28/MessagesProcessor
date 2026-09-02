using System.ComponentModel.DataAnnotations;

namespace MessagesProcessor.Messages;

public class OrderConfirmationData : BaseData
{
    [Required]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "CustomerName is required.")]
    public string CustomerName { get; set; } = string.Empty;
}
