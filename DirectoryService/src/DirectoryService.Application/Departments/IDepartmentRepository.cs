using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentRepository
{
    /// <summary>
    /// Добавление нового подразделения.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Guid созданного подразделения.</returns>
    Task<Guid> AddAsync(Department department, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохранение изменений.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат сохранения.</returns>
    Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получение подразделения по ID.
    /// </summary>
    /// <param name="id">ID подразделения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Подразделение.</returns>
    Task<Result<Department, Error>> GetByIdAsync(DepartmentId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получение подразделения по ID с блокировкой.
    /// </summary>
    /// <param name="id">ID подразделения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Подразделение.</returns>
    Task<Result<Department, Error>> GetByIdWithLockAsync(
        DepartmentId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Существует ли активное подразделение по ID.
    /// </summary>
    /// <param name="id">ID подразделения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат проверки.</returns>
    Task<bool> IsActiveDepartmentExistAsync(DepartmentId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Существует ли активное подразделение по идентификатору (проверка уникальности идентификатора).
    /// </summary>
    /// <param name="identifier">Идентификатор.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество активных подразделений.</returns>
    Task<bool> IsActiveIdentifierExistAsync(Identifier identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверка существования активных локаций.
    /// </summary>
    /// <param name="locationIds">Список локаций.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат проверки.</returns>
    Task<bool> IsActiveLocationsExistAsync(List<LocationId> locationIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаление локаций у подразделения по его ID.
    /// </summary>
    /// <param name="departmentId">ID подразделения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат удаления.</returns>
    Task<UnitResult<Error>> DeleteDepartmentLocationsByIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Является ли подразделение родителем по отношению к другому.
    /// </summary>
    /// <param name="parentPath">Путь родительского подразделения.</param>
    /// <param name="childId">ID дочернего подразделения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат проверки.</returns>
    Task<Result<bool, Error>> IsParent(string parentPath, DepartmentId childId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Блокировка подчинённых подразделений для изменений.
    /// </summary>
    /// <param name="parentPath">Путь родительского подразделения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения операции.</returns>
    Task<UnitResult<Error>> LockDescendantsAsync(string parentPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновление свойств Path и Depth у подчинённых подразделений.
    /// </summary>
    /// <param name="oldParentPath">Старый родительский путь.</param>
    /// <param name="newParentPath">Новый родительский путь.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат обновления.</returns>
    Task<UnitResult<Error>> UpdateDescendantProperties(
        string oldParentPath,
        string newParentPath,
        CancellationToken cancellationToken = default);
}