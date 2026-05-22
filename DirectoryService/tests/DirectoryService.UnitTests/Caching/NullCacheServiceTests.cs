using DirectoryService.Infrastructure.Caching;
using FluentAssertions;

namespace DirectoryService.UnitTests.Caching;

public class NullCacheServiceTests
{
    [Fact]
    public async Task GetOrCreateAsync_should_always_invoke_factory()
    {
        var sut = new NullCacheService();
        int calls = 0;

        ValueTask<int> Factory(CancellationToken ct)
        {
            calls++;
            return ValueTask.FromResult(42);
        }

        int first = await sut.GetOrCreateAsync("k", Factory);
        int second = await sut.GetOrCreateAsync("k", Factory);

        first.Should().Be(42);
        second.Should().Be(42);
        calls.Should().Be(2);
    }

    [Fact]
    public async Task RemoveAsync_should_be_noop()
    {
        var sut = new NullCacheService();

        var act = async () => await sut.RemoveAsync("any");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveByTagAsync_should_be_noop()
    {
        var sut = new NullCacheService();

        var act = async () => await sut.RemoveByTagAsync("any");

        await act.Should().NotThrowAsync();
    }
}