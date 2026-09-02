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
    private readonly IOrderConfirmation _orderConfirmation;
    private readonly IOrderDelivery _orderDelivery;
    private readonly IOrderInvoice _orderInvoice;

    public MessageProcessorFunction(ILogger<MessageProcessorFunction> logger, IOrderConfirmation orderConfirmation,
        IOrderDelivery orderDelivery, IOrderInvoice orderInvoice)
    {
        _logger = logger;
        _orderConfirmation = orderConfirmation;
        _orderDelivery = orderDelivery;
        _orderInvoice = orderInvoice;
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

            switch (dataType)
            {
                case "OrderConfirmation":
                    {
                        var data = Deserialize<OrderConfirmationData>(body);
                        
                        _logger.LogInformation("Order Confirmation Data= Order Id: {OrderId}, Quantity={Quantity}, CustomerName={CustomerName}",
                            data.OrderId, data.Quantity, data.CustomerName);
                        await _orderConfirmation.ProcessConfirmedOrders(data);
                        break;
                    }

                case "OrderDelivery":
                    {
                        var data = Deserialize<OrderDeliveryData>(body);
                       
                        _logger.LogInformation("Order Delivery Data= Order Id: {OrderId}, Customer Name={CustomerName}, Customer Address={CustomerAddress}, Customer Phone number: {CustomerPhoneNumber}",
                            data.OrderId, data.CustomerName, data.CustomerAddress, data.CustomerPhoneNumber);
                        await _orderDelivery.ProcessDeliveredOrders(data);
                        break;
                    }

                case "OrderInvoice":
                    {
                        var data = Deserialize<OrderInvoiceData>(body);
                        
                        _logger.LogInformation("Order Invoice Data= Order Id: {OrderId}, Order Amount={OrderAmount}, Billing Date={BillingDate}",
                            data.OrderId, data.OrderAmount, data.BillingDate);
                        await _orderInvoice.ProcessInvoicedOrders(data);
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
}
