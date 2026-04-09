namespace DirectoryService.Infrastructure.BackgroundServices;

public sealed class DepartmentCleanupOptions
{
    public const string SECTION_NAME = "DepartmentCleanup";

    public bool Enabled { get; init; } = true;

    public TimeSpan RunAtUtc { get; init; } = TimeSpan.FromHours(2);

    public int RetentionMonths { get; init; } = 1;
}