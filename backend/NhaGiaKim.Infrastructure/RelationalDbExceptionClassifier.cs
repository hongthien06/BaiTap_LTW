using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Application.Abstractions;

namespace NhaGiaKim.Infrastructure;

public class RelationalDbExceptionClassifier : IDbExceptionClassifier
{
    // SQL Server: 2627 = vi pham PRIMARY KEY/UNIQUE constraint, 2601 = trung khoa tren unique index.
    private const int SqlServerUniqueConstraint = 2627;
    private const int SqlServerDuplicateKey = 2601;

    public bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var inner = exception.InnerException;

        if (inner is SqlException sql)
        {
            return sql.Number is SqlServerUniqueConstraint or SqlServerDuplicateKey;
        }

        // Test tich hop chay tren SQLite. Khong tham chieu goi Sqlite vao code production
        // chi de bat mot kieu exception, nen nhan dien theo thong diep chuan cua SQLite.
        return inner is not null
            && inner.GetType().Name == "SqliteException"
            && inner.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
    }
}
