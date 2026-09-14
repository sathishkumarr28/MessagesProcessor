using MessagesProcessor.Configuration;
using MessagesProcessor.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MessagesProcessor.Services
{
    public class OrderDeliveryHandler : MessageHandlerBase<OrderDeliveryData>
    {
        public OrderDeliveryHandler(
            ILogger<OrderDeliveryHandler> logger,
            IOptions<MessageProcessorOptions> configuration,
            HttpClient httpClient)
            : base(logger, configuration, httpClient)
        {
        }

        public override string DataType => "OrderDelivery";

        // Delivery-specific extra step, runs before the order is POSTed downstream.
        protected override Task OnBeforeSendAsync(OrderDeliveryData data)
        {
            _logger.LogInformation(
                "Preparing delivery for OrderId {OrderId} to {CustomerAddress}.",
                data.OrderId,
                data.CustomerAddress);

            // TODO: add delivery-only logic here (e.g. schedule a courier,
            // enrich address, notify logistics, etc.)

            return Task.CompletedTask;
        }
    }
}
