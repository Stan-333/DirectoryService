using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.UnitTests.Caching;

/// <summary>
/// Тестовый double, имитирующий полную недоступность HybridCache —
/// каждая операция падает с исключением. Используется для проверки graceful degradation.
/// </summary>
internal sealed class ThrowingHybridCache : HybridCache
{
    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("simulated cache failure");

    public override ValueTask SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("simulated cache failure");

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("simulated cache failure");

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("simulated cache failure");
}