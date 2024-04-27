using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Schemata.Entity.EntityFrameworkCore;

public static class QueryableExtensions
{
#if NETSTANDARD2_0 || NETSTANDARD2_1
    public static string? ToSql<TEntity>(this IQueryable<TEntity> query)
    {
        using var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
        var       relationalCommandCache = enumerator.Private("_relationalCommandCache");
        var       selectExpression = relationalCommandCache?.Private<SelectExpression>("_selectExpression");
        var       factory = relationalCommandCache?.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

        var sqlGenerator = factory?.Create();
        var command      = sqlGenerator?.GetCommand(selectExpression);

        var sql = command?.CommandText;
        return sql;
    }

    private static object? Private(this object? obj, string privateField) {
        return obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
    }

    private static T? Private<T>(this object obj, string privateField) {
        return (T?)obj.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
    }
#endif
}
