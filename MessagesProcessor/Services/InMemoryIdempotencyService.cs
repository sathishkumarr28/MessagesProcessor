using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace MessagesProcessor.Services;

public class InMemoryIdempotencyService : IIdempotencyService
{
    private static readonly ConcurrentDictionary<string, string> Store = new();

    private readonly ILogger<InMemoryIdempotencyService> _logger;

    public InMemoryIdempotencyService(ILogger<InMemoryIdempotencyService> logger)
    {
        _logger = logger;
    }

    public Task<bool> TryReserveAsync(string key)
    {
        var added = Store.TryAdd(key, key);

        if (added)
        {
            _logger.LogInformation("Idempotency key '{Key}' reserved for processing.", key);
        }
        else
        {
            _logger.LogWarning("Idempotency key '{Key}' already exists. Message treated as duplicate.", key);
        }

        return Task.FromResult(added);
    }

    public Task MarkProcessedAsync(string key)
    {
        _logger.LogInformation("Idempotency key '{Key}' marked as processed.", key);
        return Task.CompletedTask;
    }

    public Task ReleaseAsync(string key)
    {
        if (Store.TryRemove(key, out _))
        {
            _logger.LogInformation("Idempotency key '{Key}' released for retry.", key);
        }

        return Task.CompletedTask;
    }
}
