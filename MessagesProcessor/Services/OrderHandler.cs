using MessagesProcessor.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace MessagesProcessor.Services
{
    public class OrderHandler : IOrderHandler
    {
        private readonly ILogger<OrderHandler> _logger;
        private readonly IOptions<MessageProcessorOptions> _configuration;
        private readonly HttpClient _httpClient;
        public OrderHandler(ILogger<OrderHandler> logger, IOptions<MessageProcessorOptions> configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }
        public async Task HandleOrders<T>(T orderData)
        {
            try
            {
                _logger.LogInformation("Processing orders of type {OrderType}.", typeof(T).Name);
                var getUrl = _configuration.Value.EndpointUrls[typeof(T).Name];

                var response = await _httpClient.PostAsJsonAsync(getUrl, orderData);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process orders of type {OrderType}.", typeof(T).Name);
                throw;
            }
        }
    }
}
