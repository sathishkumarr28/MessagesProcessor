using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using MessagesProcessor.Messages;
using MessagesProcessor.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MessagesProcessor;

public class MessageProcessorFunction
{
    private readonly ILogger<MessageProcessorFunction> _logger;
    private readonly IIdempotencyService _idempotencyService;
    private readonly Dictionary<string, IMessageHandler> _handlers = new();

    public MessageProcessorFunction(
        ILogger<MessageProcessorFunction> logger,
        IEnumerable<IMessageHandler> handlers,
        IIdempotencyService idempotencyService)
    {
        _logger = logger;
        _idempotencyService = idempotencyService;
        _handlers = handlers.ToDictionary(h => h.DataType);
    }

    [Function(nameof(MessageProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger("order-topic", "order-topic-subscription", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message Body: {Body}", message.Body);
        _logger.LogInformation("Message Content-Type: {ContentType}", message.ContentType);

        var body = message.Body.ToString();

        string? dataType;
        try
        {
            using var document = JsonDocument.Parse(body);
            dataType = document.RootElement.TryGetProperty("dataType", out var dataTypeElement)
                ? dataTypeElement.GetString()
                : null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse message body as JSON.");
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "Invalid JSON", deadLetterErrorDescription: "The message body could not be parsed as valid JSON.");
            return;
        }

        var idempotencyKey = message.MessageId;

        try
        {
            if (dataType is null || !_handlers.TryGetValue(dataType, out var handler))
            {
                _logger.LogWarning("No handler found for message type '{MessageType}'.", dataType);
                await _idempotencyService.ReleaseAsync(idempotencyKey);
                await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "HandlerNotFound", deadLetterErrorDescription: $"No handler found for message type '{dataType}'.");
                return;
            }

            if (!await _idempotencyService.TryReserveAsync(idempotencyKey))
            {
                _logger.LogInformation("Duplicate message detected for key '{Key}'. Skipping processing.", idempotencyKey);
                await messageActions.CompleteMessageAsync(message);
                return;
            }

            await handler.HandleAsync(body);

            await _idempotencyService.MarkProcessedAsync(idempotencyKey);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed for message type '{MessageType}'.", dataType);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "ValidationFailed", deadLetterErrorDescription: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to process the order");
            // Release the reservation so the message can be retried on redelivery.
            throw;
        }
        finally
        {
            await _idempotencyService.ReleaseAsync(idempotencyKey);
        }
    }
}
