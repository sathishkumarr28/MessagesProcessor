using MessagesProcessor.Configuration;
using MessagesProcessor.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services
    .AddOptions<MessageProcessorOptions>()
    .BindConfiguration(MessageProcessorOptions.SectionName);

// In-memory idempotency store for this simple/local solution.
builder.Services.AddSingleton<IIdempotencyService, InMemoryIdempotencyService>();
builder.Services.AddSingleton<IMessageHandler, OrderConfirmationHandler>();
builder.Services.AddSingleton<IMessageHandler, OrderDeliveryHandler>();
builder.Services.AddSingleton<IMessageHandler, OrderInvoiceHandler>();

builder.Services
    .AddHttpClient();

builder.Build().Run();
