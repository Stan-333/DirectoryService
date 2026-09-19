namespace DirectoryService.Infrastructure.Database;

/// <summary>
/// Имена индексов БД. Используются в конфигурациях EF Core и в <see cref="PostgresErrorMapper"/>,
/// чтобы сообщение о конфликте уникальности не разошлось с настоящим именем индекса.
/// В миграциях имена намеренно остаются строками: миграция фиксирует схему на момент создания
/// и не должна меняться вместе с константами.
/// </summary>
internal static class IndexNames
{
    public const string DEPARTMENT_IDENTIFIER = "idx_department_identifier";

    public const string DEPARTMENT_PATH = "idx_department_path";

    public const string LOCATION_NAME = "idx_location_name";

    /// <summary>
    /// Создаётся только SQL-миграциями (AddAddressIndex, NormalizeNullableLocationAddressUniqueness):
    /// частичный составной индекс по полям адреса с COALESCE(apartment, '') не описать в модели EF Core.
    /// </summary>
    public const string LOCATION_ADDRESS = "idx_location_address";

    public const string POSITION_NAME = "idx_position_name";
}