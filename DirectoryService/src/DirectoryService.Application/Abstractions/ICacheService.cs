namespace DirectoryService.Application.Abstractions;

/// <summary>
/// Абстракция кэша для бизнес-слоя.
/// Реализация должна гарантировать graceful degradation: при недоступности удалённого
/// кэша операции не выбрасывают исключения — на чтение возвращается результат фабрики,
/// на инвалидацию ошибка молча логируется.
/// </summary>
public interface ICacheService
{
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? ttl = null,
        IReadOnlyList<string>? tags = null,
        CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
}