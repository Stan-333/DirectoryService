using System.Data.Common;
using DirectoryService.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Database;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DirectoryServiceDbContext _dbContext;

    public DbConnectionFactory(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        return connection;
    }
}