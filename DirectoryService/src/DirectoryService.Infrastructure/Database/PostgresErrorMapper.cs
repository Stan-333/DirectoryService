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
            IndexNames.DEPARTMENT_IDENTIFIER => Error.Conflict(
                "record.already.exist",
                "Подразделение с таким идентификатором уже существует"),
            IndexNames.LOCATION_NAME => Error.Conflict(
                "record.already.exist",
                "Локация с таким именем уже существует"),
            IndexNames.LOCATION_ADDRESS => Error.Conflict(
                "record.already.exist",
                "Локация с таким адресом уже существует"),
            IndexNames.POSITION_NAME => Error.Conflict(
                "record.already.exist",
                "Должность с таким именем уже существует"),
            _ => GeneralErrors.AlreadyExist(),
        };
    }
}