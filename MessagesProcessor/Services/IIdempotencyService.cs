namespace MessagesProcessor.Services;

public interface IIdempotencyService
{   
    Task<bool> TryReserveAsync(string key);

    Task MarkProcessedAsync(string key);

    Task ReleaseAsync(string key);
}
