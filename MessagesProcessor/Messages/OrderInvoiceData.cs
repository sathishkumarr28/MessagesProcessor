using System.ComponentModel.DataAnnotations;

namespace MessagesProcessor.Messages;

public class OrderInvoiceData : BaseData
{
    public double OrderAmount { get; set; }

    [Required(ErrorMessage = "InvoiceNumber is required.")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "BillingDate is required.")]
    public DateTime BillingDate { get; set; }
}
