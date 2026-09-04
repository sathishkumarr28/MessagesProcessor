using System.ComponentModel.DataAnnotations;

namespace MessagesProcessor.Messages;

public class OrderInvoiceData : BaseData
{
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "OrderAmount must be a non-negative number.")]
    public decimal OrderAmount { get; set; }

    [Required(ErrorMessage = "InvoiceNumber is required.")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Range(typeof(DateTime), "1/1/2020", "12/31/2099", ErrorMessage = "BillingDate must be a valid date.")]
    public DateTime BillingDate { get; set; }
}
