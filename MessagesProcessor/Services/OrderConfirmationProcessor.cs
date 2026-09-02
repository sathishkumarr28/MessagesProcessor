using MessagesProcessor.Configuration;
using MessagesProcessor.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace MessagesProcessor.Services
{
    public class OrderConfirmationProcessor : IOrderConfirmation
    {
        private readonly ILogger<OrderConfirmationProcessor> _logger;
        private readonly HttpClient _httpClient;
        private readonly IOptions<MessageProcessorOptions> _configuration;
        public OrderConfirmationProcessor(ILogger<OrderConfirmationProcessor> logger, HttpClient httpClient, IOptions<MessageProcessorOptions> configuration) 
        { 
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task ProcessConfirmedOrders(OrderConfirmationData orderConfirmationData)
        {
            try
            {
                _logger.LogInformation("Processing confirmed orders.");
                var getUrl = _configuration.Value.EndpointUrls["OrderConfirmation"];

                await _httpClient.PostAsJsonAsync(getUrl, orderConfirmationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process confirmed orders");
            }
        }
    }
}
