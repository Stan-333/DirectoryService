using DirectoryService.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DirectoryService.UnitTests.Caching;

public class HybridCacheServiceTests
{
    [Fact]
    public async Task GetOrCreateAsync_with_miss_should_invoke_factory_and_return_value()
    {
        var sut = CreateSut(CreateRealHybridCache());
        int calls = 0;

        string value = await sut.GetOrCreateAsync(
            "key-1",
            _ =>
            {
                calls++;
                return ValueTask.FromResult("payload");
            });

        value.Should().Be("payload");
        calls.Should().Be(1);
    }

    [Fact]
    public async Task GetOrCreateAsync_with_hit_should_skip_factory_on_second_call()
    {
        var sut = CreateSut(CreateRealHybridCache());
        int calls = 0;

        ValueTask<string> Factory(CancellationToken ct)
        {
            calls++;
            return ValueTask.FromResult("payload");
        }

        await sut.GetOrCreateAsync("key-2", Factory);
        await sut.GetOrCreateAsync("key-2", Factory);

        calls.Should().Be(1);
    }

    [Fact]
    public async Task RemoveByTagAsync_should_invalidate_tagged_entries()
    {
        var sut = CreateSut(CreateRealHybridCache());
        int calls = 0;
        string[] tags = new[] { "departments" };

        ValueTask<string> Factory(CancellationToken ct)
        {
            calls++;
            return ValueTask.FromResult($"payload-{calls}");
        }

        string first = await sut.GetOrCreateAsync("key-3", Factory, tags: tags);
        await sut.RemoveByTagAsync("departments");
        string second = await sut.GetOrCreateAsync("key-3", Factory, tags: tags);

        first.Should().Be("payload-1");
        second.Should().Be("payload-2");
        calls.Should().Be(2);
    }

    [Fact]
    public async Task GetOrCreateAsync_should_fall_back_to_factory_when_cache_throws()
    {
        var sut = CreateSut(new ThrowingHybridCache());
        int calls = 0;

        string value = await sut.GetOrCreateAsync(
            "key-x",
            _ =>
            {
                calls++;
                return ValueTask.FromResult("from-db");
            });

        value.Should().Be("from-db");
        calls.Should().Be(1);
    }

    [Fact]
    public async Task RemoveAsync_should_swallow_cache_errors()
    {
        var sut = CreateSut(new ThrowingHybridCache());

        var act = async () => await sut.RemoveAsync("any");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveByTagAsync_should_swallow_cache_errors()
    {
        var sut = CreateSut(new ThrowingHybridCache());

        var act = async () => await sut.RemoveByTagAsync("departments");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetOrCreateAsync_should_propagate_OperationCanceledException()
    {
        var sut = CreateSut(CreateRealHybridCache());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await sut.GetOrCreateAsync<string>(
            "key-cancel",
            _ => throw new OperationCanceledException(cts.Token),
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private static HybridCache CreateRealHybridCache()
    {
        // Реальный HybridCache без L2 — достаточно для проверки cache-aside и тегов
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHybridCache();
        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    private static HybridCacheService CreateSut(HybridCache cache, ILogger<HybridCacheService>? logger = null)
        => new(cache, logger ?? NullLogger<HybridCacheService>.Instance);
}