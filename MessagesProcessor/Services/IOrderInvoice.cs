using MessagesProcessor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagesProcessor.Services
{
    public interface IOrderInvoice
    {
        Task ProcessInvoicedOrders(OrderInvoiceData orderInvoiceData);
    }
}
