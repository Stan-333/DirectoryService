using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionRepository
{
    /// <summary>
    /// Добавление новой должности
    /// </summary>
    /// <param name="position">Должность.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Guid созданной должности.</returns>
    Task<Guid> AddAsync(Position position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохранение изменений
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат сохранения.</returns>
    Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получение должности по id
    /// </summary>
    /// <param name="id">Id должности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения операции.</returns>
    Task<Result<Position, Errors>> GetByIdAsync(PositionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Существует ли активная должность по названию (проверка уникальности названия)
    /// </summary>
    /// <param name="name">Название должности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество активных должностей.</returns>
    Task<bool> IsActiveDepartmentNameExistAsync(PositionName name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверка существования активного подразделения
    /// </summary>
    /// <param name="departmentId">Id подразделения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат проверки.</returns>
    Task<bool> IsActiveDepartmentExistAsync(DepartmentId departmentId, CancellationToken cancellationToken = default);
}