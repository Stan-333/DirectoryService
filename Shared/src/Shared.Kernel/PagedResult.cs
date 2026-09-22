using System.Text.Json.Serialization;

namespace Shared.Kernel;

/// <summary>
/// Одна страница списка вместе со сведениями о пагинации.
/// В JSON передаются и вычисляемые свойства (<see cref="TotalPages"/>, <see cref="HasNextPage"/>),
/// чтобы клиенту не пришлось считать их самому. При чтении JSON они пересчитываются.
/// </summary>
/// <typeparam name="T">Тип элемента списка.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Создаёт страницу списка.
    /// </summary>
    /// <param name="items">Элементы текущей страницы.</param>
    /// <param name="totalCount">Сколько элементов подходит под запрос на всех страницах.</param>
    /// <param name="page">Номер страницы, начиная с 1.</param>
    /// <param name="pageSize">Сколько элементов помещается на страницу.</param>
    [JsonConstructor]
    public PagedResult(IReadOnlyList<T> items, long totalCount, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Элементы текущей страницы.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Сколько элементов подходит под запрос на всех страницах.
    /// </summary>
    public long TotalCount { get; }

    /// <summary>
    /// Номер страницы, начиная с 1.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Сколько элементов помещается на страницу.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Сколько всего страниц. Для пустого списка — 0.
    /// </summary>
    public long TotalPages => (TotalCount / PageSize) + (TotalCount % PageSize == 0 ? 0 : 1);

    /// <summary>
    /// Есть ли страница перед текущей.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Есть ли страница после текущей.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}