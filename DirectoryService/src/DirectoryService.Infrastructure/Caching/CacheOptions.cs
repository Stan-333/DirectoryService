namespace DirectoryService.Infrastructure.Caching;

public class CacheOptions
{
    public const string SECTION_NAME = "Caching";

    public bool Enabled { get; set; } = true;
}