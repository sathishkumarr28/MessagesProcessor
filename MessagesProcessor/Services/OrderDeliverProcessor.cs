using MessagesProcessor.Configuration;
using MessagesProcessor.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace MessagesProcessor.Services
{
    public class OrderDeliverProcessor : IOrderDelivery
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderDeliverProcessor> _logger;
        private readonly IOptions<MessageProcessorOptions> _configuration;
        public OrderDeliverProcessor(ILogger<OrderDeliverProcessor> logger, HttpClient httpClient, IOptions<MessageProcessorOptions> configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task ProcessDeliveredOrders(OrderDeliveryData orderDeliveryData)
        {
            try
            {
                _logger.LogInformation("Processing delivered orders.");
                var getUrl = _configuration.Value.EndpointUrls["OrderDelivery"];
                await _httpClient.PostAsJsonAsync(getUrl, orderDeliveryData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to deliver order");
            }
        }
    }
}
