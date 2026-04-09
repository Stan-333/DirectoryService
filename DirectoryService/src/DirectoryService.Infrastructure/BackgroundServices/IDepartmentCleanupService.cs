namespace DirectoryService.Infrastructure.BackgroundServices;

public interface IDepartmentCleanupService
{
    Task<int> CleanupExpiredDepartmentsAsync(CancellationToken cancellationToken = default);
}