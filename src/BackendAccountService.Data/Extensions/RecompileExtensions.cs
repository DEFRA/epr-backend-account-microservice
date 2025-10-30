using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAccountService.Data.Extensions;

[ExcludeFromCodeCoverage]
public static class RecompileExtensions
{
    private const string RecompileTag = "recompile_query_tag";
    private const string RecompileComment = $"-- {RecompileTag}\r\n";

    public static DbContextOptionsBuilder UseRecompileExtensions(this DbContextOptionsBuilder builder)
    {
        return builder.AddInterceptors(RecompileInterceptor.Instance);
    }

    public static IQueryable<T> WithRecompile<T>(this IQueryable<T> query)
    {
        return query.TagWith(RecompileTag);
    }

    private sealed class RecompileInterceptor : DbCommandInterceptor
    {
        public readonly static RecompileInterceptor Instance = new();

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CorrectCommand(command);

            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            CorrectCommand(command);

            return base.ReaderExecuting(command, eventData, result);
        }

        private static void CorrectCommand(DbCommand command)
        {
            if (!command.CommandText.Contains(RecompileComment))
            {
                return;
            }

            // if query was changed, we have to append RECOMPILE option
            var newQuery = command.CommandText.Replace(RecompileComment, "");

            // remove rest of the comment
            if (newQuery.StartsWith("\r\n"))
            {
                newQuery = newQuery.Substring(2);
            }

#pragma warning disable S2077
            command.CommandText = newQuery + "\r\nOPTION(RECOMPILE)";
#pragma warning restore S2077
        }
    }
}
