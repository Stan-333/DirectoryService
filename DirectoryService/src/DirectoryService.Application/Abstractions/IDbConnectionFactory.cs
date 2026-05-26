using System.Data.Common;

namespace DirectoryService.Application.Abstractions;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}