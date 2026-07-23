namespace DirectoryService.Contracts;

public static class PaginationConstraints
{
    public const int MinPage = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;

    public static int NormalizePage(int page, int pageSize)
    {
        int normalizedPageSize = NormalizePageSize(pageSize);
        long maxPageForIntOffset = ((long)int.MaxValue / normalizedPageSize) + 1;
        long maxPage = Math.Min(maxPageForIntOffset, int.MaxValue);

        return (int)Math.Clamp((long)page, MinPage, maxPage);
    }

    public static int NormalizePageSize(int pageSize)
    {
        return Math.Clamp(pageSize, MinPageSize, MaxPageSize);
    }

    public static int CalculateOffset(int page, int pageSize)
    {
        int normalizedPageSize = NormalizePageSize(pageSize);
        int normalizedPage = NormalizePage(page, normalizedPageSize);
        long offset = ((long)normalizedPage - 1) * normalizedPageSize;

        return checked((int)offset);
    }
}