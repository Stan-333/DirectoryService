using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    /// <summary>
    /// Добавление новой локации
    /// </summary>
    /// <param name="location">Локация.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Guid созданной локации.</returns>
    Task<Guid> AddAsync(Location location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохранение изменений
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат сохранения.</returns>
    Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Есть ли активная локация по названию (проверка уникальности названия)
    /// </summary>
    /// <param name="locationName">Название локации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество локаций.</returns>
    Task<bool> IsActiveLocationNameExistAsync(LocationName locationName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Существует ли активная локация по адресу (проверка уникальности адреса)
    /// </summary>
    /// <param name="address">Адрес.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество локаций.</returns>
    Task<bool> IsActiveAddressExistAsync(Address address, CancellationToken cancellationToken = default);
}