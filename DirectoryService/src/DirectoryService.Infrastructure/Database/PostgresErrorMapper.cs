using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Database;

internal static class PostgresErrorMapper
{
    public static Error? MapUniqueViolation(
        DbUpdateException exception,
        out string? constraintName)
    {
        constraintName = null;

        if (exception.GetBaseException() is not PostgresException postgresException
            || postgresException.SqlState != PostgresErrorCodes.UniqueViolation)
        {
            return null;
        }

        constraintName = postgresException.ConstraintName;

        return constraintName switch
        {
            "idx_department_identifier" => Error.Conflict(
                "record.already.exist",
                "Подразделение с таким идентификатором уже существует"),
            "idx_location_name" => Error.Conflict(
                "record.already.exist",
                "Локация с таким именем уже существует"),
            "idx_location_address" => Error.Conflict(
                "record.already.exist",
                "Локация с таким адресом уже существует"),
            "idx_position_name" => Error.Conflict(
                "record.already.exist",
                "Должность с таким именем уже существует"),
            _ => GeneralErrors.AlreadyExist(),
        };
    }
}