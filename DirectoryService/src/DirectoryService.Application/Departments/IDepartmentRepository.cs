using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentRepository
{
    /// <summary>
    /// Добавление нового подразделения
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Guid созданного подразделения.</returns>
    Task<Guid> AddAsync(Department department, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохранение изменений
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат сохранения.</returns>
    Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получение подразделения по id
    /// </summary>
    /// <param name="id">Id подразделения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Подразделение.</returns>
    Task<Result<Department, Error>> GetByIdAsync(DepartmentId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Существует ли активное подразделение по идентификатору (проверка уникальности идентификатора)
    /// </summary>
    /// <param name="identifier">Идентификатор.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество активных подразделений.</returns>
    Task<bool> IsActiveIdentifierExistAsync(Identifier identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверка существования активных локаций
    /// </summary>
    /// <param name="locationIds">Список локаций.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат проверки.</returns>
    Task<bool> IsActiveLocationsExistAsync(List<LocationId> locationIds, CancellationToken cancellationToken = default);
}