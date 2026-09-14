using MessagesProcessor.Configuration;
using MessagesProcessor.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MessagesProcessor.Services
{
    public class OrderConfirmationHandler : MessageHandlerBase<OrderConfirmationData>
    {
        public OrderConfirmationHandler(
            ILogger<OrderConfirmationHandler> logger,
            IOptions<MessageProcessorOptions> configuration,
            HttpClient httpClient)
            : base(logger, configuration, httpClient)
        {
        }

        public override string DataType => "OrderConfirmation";
    }
}
