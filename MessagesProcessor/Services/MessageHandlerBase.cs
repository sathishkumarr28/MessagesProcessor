using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using MessagesProcessor.Configuration;
using MessagesProcessor.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MessagesProcessor.Services
{
    public abstract class MessageHandlerBase<T> : IMessageHandler
        where T : BaseData, new()
    {        
        protected readonly ILogger _logger;
        private readonly IOptions<MessageProcessorOptions> _configuration;
        private readonly HttpClient _httpClient;

        protected MessageHandlerBase(
            ILogger logger,
            IOptions<MessageProcessorOptions> configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public abstract string DataType { get; }

        public async Task HandleAsync(string body)
        {
            var data = Deserialize(body);
            ValidateData(data);

            try
            {
                _logger.LogInformation("Processing orders of type {OrderType}.", typeof(T).Name);

                await OnBeforeSendAsync(data);

                var getUrl = _configuration.Value.EndpointUrls[typeof(T).Name];

                var response = await _httpClient.PostAsJsonAsync(getUrl, data);
                response.EnsureSuccessStatusCode();

                await OnAfterSendAsync(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process orders of type {OrderType}.", typeof(T).Name);
                throw;
            }
        }

        // Extension points (Template Method hooks).
        // Override these in a concrete handler to add type-specific behavior.
        // The default implementations do nothing.
        protected virtual Task OnBeforeSendAsync(T data) => Task.CompletedTask;

        protected virtual Task OnAfterSendAsync(T data) => Task.CompletedTask;

        private static T Deserialize(string body)
        {
            var envelope = JsonSerializer.Deserialize<SystemMessage<T>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return envelope?.Data
                ?? throw new JsonException("Envelope 'data' payload is missing.");
        }

        private static void ValidateData(T data)
        {
            var validationContext = new ValidationContext(data);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(data, validationContext, validationResults, true))
            {
                var errorMessages = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                throw new ValidationException($"Validation failed for {typeof(T).Name}: {errorMessages}");
            }
        }
    }
}
