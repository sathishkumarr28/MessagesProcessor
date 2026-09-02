using MessagesProcessor.Configuration;
using MessagesProcessor.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace MessagesProcessor.Services
{
    public class OrderInvoiceProcessor : IOrderInvoice
    {
        private readonly ILogger<OrderInvoiceProcessor> _logger;
        private readonly HttpClient _httpClient;
        private readonly IOptions<MessageProcessorOptions> _configuration;

        public OrderInvoiceProcessor(ILogger<OrderInvoiceProcessor> logger, HttpClient httpClient, IOptions<MessageProcessorOptions> configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task ProcessInvoicedOrders(OrderInvoiceData orderInvoiceData)
        {
            try
            {
                _logger.LogInformation("Processing invoiced orders.");
                var getUrl = _configuration.Value.EndpointUrls["OrderInvoice"]; 
                await _httpClient.PostAsJsonAsync(getUrl, orderInvoiceData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to generate Invoice");
            }
        }
    }
}
