using DirectoryService.Application.Abstractions;

namespace DirectoryService.Infrastructure.Caching;

/// <summary>
/// No-op реализация для отключённого кэша (Caching:Enabled = false).
/// Всегда обращается напрямую к фабрике, инвалидация — заглушка.
/// </summary>
public class NullCacheService : ICacheService
{
    public ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? ttl = null,
        IReadOnlyList<string>? tags = null,
        CancellationToken cancellationToken = default)
        => factory(cancellationToken);

    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}