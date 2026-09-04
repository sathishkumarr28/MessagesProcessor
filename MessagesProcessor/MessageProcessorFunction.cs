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
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<MessageProcessorFunction> _logger;
    private readonly IOrderHandler _orderHandler;

    public MessageProcessorFunction(ILogger<MessageProcessorFunction> logger, IOrderHandler orderHandler)
    {
        _logger = logger;
        _orderHandler = orderHandler;
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

        //PKCE
        //idempotency
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

        try
        {
            switch (dataType)
            {
                case "OrderConfirmation":
                    {
                        var data = Deserialize<OrderConfirmationData>(body);
                        ValidateData(data);
                        _logger.LogInformation("Order Confirmation Data= Order Id: {OrderId}, Quantity={Quantity}, CustomerName={CustomerName}",
                            data.OrderId, data.Quantity, data.CustomerName);
                        await _orderHandler.HandleOrders(data);
                        break;
                    }

                case "OrderDelivery":
                    {
                        var data = Deserialize<OrderDeliveryData>(body);
                        ValidateData(data);
                        _logger.LogInformation("Order Delivery Data= Order Id: {OrderId}, Customer Name={CustomerName}, Customer Address={CustomerAddress}, Customer Phone number: {CustomerPhoneNumber}",
                            data.OrderId, data.CustomerName, data.CustomerAddress, data.CustomerPhoneNumber);
                        await _orderHandler.HandleOrders(data);
                        break;
                    }

                case "OrderInvoice":
                    {
                        var data = Deserialize<OrderInvoiceData>(body);
                        ValidateData(data);
                        _logger.LogInformation("Order Invoice Data= Order Id: {OrderId}, Order Amount={OrderAmount}, Billing Date={BillingDate}",
                            data.OrderId, data.OrderAmount, data.BillingDate);
                        await _orderHandler.HandleOrders(data);
                        break;
                    }
                default:
                    {
                        _logger.LogError("Message type '{MessageType}' not supported.", dataType);
                        await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "Unsupported message type", deadLetterErrorDescription: $"The message type '{dataType}' is not supported.");
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to process the order");
            throw;
        }
    }

    private static T Deserialize<T>(string body) where T : BaseData, new()
    {
        var envelope = JsonSerializer.Deserialize<SystemMessage<T>>(body, SerializerOptions);
        return envelope?.Data
            ?? throw new JsonException("Envelope 'data' payload is missing.");
    }

    private static void ValidateData<T>(T data) where T : BaseData
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
