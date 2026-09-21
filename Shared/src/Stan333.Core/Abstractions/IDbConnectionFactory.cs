using System.Data.Common;

namespace Stan333.Core.Abstractions;

public interface IDbConnectionFactory
{
    /// <summary>
    /// Возвращает открытое соединение текущей единицы работы (того же DbContext),
    /// поэтому запросы через него видят открытую транзакцию.
    /// Соединением владеет DbContext: закрывать и освобождать его нельзя.
    /// </summary>
    Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default);
}