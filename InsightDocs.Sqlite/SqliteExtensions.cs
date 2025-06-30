using InsightDocs.Abstractions;
using InsightDocs.Sqlite.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.Sqlite;

public static class SqliteExtensions
{
    public static InsightDocsBuilder UseSqliteSearch(this InsightDocsBuilder builder, Action<SqliteSearchServiceOptions>? optionsFactory = null)
    {
        builder.Services.AddSingleton<ISearchService, SqliteSearchService>();

        if (optionsFactory != null)
        {
            builder.Services.AddSingleton((serviceProvider) =>
            {
                SqliteSearchServiceOptions options = new SqliteSearchServiceOptions();
                optionsFactory(options);

                return options;
            });
        }

        else
        {
            builder.Services.AddSingleton(new SqliteSearchServiceOptions());
        }

        return builder;
    }

    public static InsightDocsBuilder PublishToSqliteDatabase(this InsightDocsBuilder builder, Action<SqlitePublisherOptions>? optionsFactory = null)
    {
        builder.Services.AddSingleton<IPublisher, SqlitePublisher>();

        if (optionsFactory != null)
        {
            builder.Services.AddSingleton((serviceProvider) =>
            {
                SqlitePublisherOptions options = new SqlitePublisherOptions();
                optionsFactory(options);

                return options;
            });
        }

        else
        {
            builder.Services.AddSingleton(new SqlitePublisherOptions());
        }

        return builder;
    }

    public static IApplicationBuilder UseInsightDocsSqliteMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<SqliteMiddleware>();
        return builder;
    }
}
