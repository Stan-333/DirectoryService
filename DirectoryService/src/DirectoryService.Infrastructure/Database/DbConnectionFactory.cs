using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;

namespace DirectoryService.Infrastructure.Database;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DirectoryServiceDbContext _dbContext;

    public DbConnectionFactory(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        return connection;
    }
}