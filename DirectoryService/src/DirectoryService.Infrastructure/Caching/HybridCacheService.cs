using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Caching;

/// <summary>
/// Реализация <see cref="ICacheService"/> поверх HybridCache (L1 в памяти + L2 в Redis).
/// Все обращения обёрнуты так, что недоступность удалённого кэша не валит запрос:
/// на чтение возвращается результат фабрики, на запись/инвалидацию — warning в лог.
/// </summary>
public class HybridCacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<HybridCacheService> _logger;

    public HybridCacheService(HybridCache hybridCache, ILogger<HybridCacheService> logger)
    {
        _hybridCache = hybridCache;
        _logger = logger;
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? ttl = null,
        IReadOnlyList<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        HybridCacheEntryOptions? options = ttl is null
            ? null
            : new HybridCacheEntryOptions { Expiration = ttl };

        // Оборачиваем фабрику, чтобы отличить исключение БД от инфраструктурной ошибки кэша.
        // Если исключение пришло из factory — мы не должны:
        //  - залогировать его как «кэш недоступен» (это ошибка БД)
        //  - повторно вызывать factory (двойная нагрузка на БД и риск двойного побочного эффекта)
        bool factoryThrew = false;

        async ValueTask<T> InstrumentedFactory(CancellationToken ct)
        {
            try
            {
                return await factory(ct);
            }
            catch
            {
                factoryThrew = true;
                throw;
            }
        }

        try
        {
            return await _hybridCache.GetOrCreateAsync(
                key,
                InstrumentedFactory,
                options,
                tags,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (!factoryThrew)
        {
            _logger.LogWarning(
                ex,
                "Кэш недоступен при чтении ключа {CacheKey} — выполняем запрос напрямую",
                key);
            return await factory(cancellationToken);
        }
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hybridCache.RemoveAsync(key, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Кэш недоступен при инвалидации ключа {CacheKey} — пропускаем",
                key);
        }
    }

    public async ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hybridCache.RemoveByTagAsync(tag, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Кэш недоступен при инвалидации по тегу {CacheTag} — пропускаем",
                tag);
        }
    }
}