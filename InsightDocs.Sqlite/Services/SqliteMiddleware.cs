using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using InsightDocs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace InsightDocs.Sqlite.Services;

public class SqliteMiddlewareOptions
{
    public string? DatabasePath
    {
        get;
        set;
    }
}

public class SqliteMiddleware
{
    protected readonly RequestDelegate _next;

    protected string DatabasePath
    {
        get;
        set;
    }

    protected ILocalSearchService? LocalSearchService
    {
        get;
        set;
    }

    protected string? SearchUrl
    {
        get;
        set;
    }

    public SqliteMiddleware(RequestDelegate next, IHostingEnvironment env, SqliteMiddlewareOptions options, IServiceProvider serviceProvider)
    {
        _next = next;

        if (String.IsNullOrEmpty(options.DatabasePath))
        {
            throw new ArgumentException("DatabasePath must be provided in SqliteMiddlewareOptions.");
        }

        DatabasePath = options.DatabasePath;

        if (!Path.IsPathRooted(DatabasePath))
        {
            DatabasePath = Path.GetFullPath(Path.Combine(env.ContentRootPath, DatabasePath));
        }

        if (!File.Exists(DatabasePath))
        {
            throw new FileNotFoundException($"The database file '{DatabasePath}' does not exist.");
        }

        ISearchService? searchService = serviceProvider.GetService<ISearchService>();

        if (searchService is ILocalSearchService localSearchService)
        {
            LocalSearchService = localSearchService;
            SearchUrl = searchService.SearchUrl;
        }
    }

    protected SqliteConnection GetConnection()
    {
        SqliteConnection connection = new SqliteConnection($"Data Source={DatabasePath};Mode=ReadOnly");
        connection.Open();

        return connection;
    }

    public async Task Invoke(HttpContext context)
    {
        string path = context.Request.Path.ToString();

        if (path == "/")
        {
            path = "/index.html";
        }

        if (!String.IsNullOrEmpty(SearchUrl) && path == SearchUrl && LocalSearchService != null && context.Request.Method == "GET")
        {
            if (context.Request.Query == null || !context.Request.Query.ContainsKey("q"))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Search query not received in the request");
                return;
            }

            List<JsonSearchResult> results = await LocalSearchService.ExecuteSearch(context.Request.Query["q"].ToString());

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;

            await JsonSerializer.SerializeAsync(context.Response.Body, results);

            return;
        }

        using (SqliteConnection connection = GetConnection())
        using (SqliteCommand command = new SqliteCommand("SELECT MimeType, DataIsGzipped, DataContentLength, Data FROM Urls JOIN MimeTypes ON Urls.MimeTypeId = MimeTypes.MimeTypeId WHERE url = @url", connection))
        {
            command.Parameters.AddWithValue("@url", path);

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    string mimeType = reader.GetString(0);
                    bool dataIsGzipped = reader.GetBoolean(1);
                    long dataContentLength = reader.GetInt64(2);
                    byte[] data = reader.GetFieldValue<byte[]>(3);

                    context.Response.ContentType = mimeType;
                    context.Response.ContentLength = dataContentLength;

                    if (mimeType.StartsWith("text/"))
                    {
                        context.Response.ContentType += "; charset=utf-8";
                    }

                    if (dataIsGzipped)
                    {
                        context.Response.Headers.Add("Content-Encoding", "gzip");
                    }

                    await context.Response.Body.WriteAsync(data);

                    return;
                }
            }
        }

        await _next(context);
    }
}
