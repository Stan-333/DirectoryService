using System.Data.Common;

namespace Stan333.Core.Abstractions;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}