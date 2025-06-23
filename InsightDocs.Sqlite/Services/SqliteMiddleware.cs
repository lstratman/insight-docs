using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;

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

    public SqliteMiddleware(RequestDelegate next, IHostingEnvironment env, SqliteMiddlewareOptions options)
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

        using (SqliteConnection connection = GetConnection())
        using (SqliteCommand command = new SqliteCommand("SELECT MimeType, DataContentLength, Data FROM Urls WHERE url = @url", connection))
        {
            command.Parameters.AddWithValue("@url", path);

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    string mimeType = reader.GetString(0);
                    long dataContentLength = reader.GetInt64(1);
                    byte[] data = reader.GetFieldValue<byte[]>(2);

                    context.Response.ContentType = mimeType;
                    context.Response.ContentLength = dataContentLength;

                    if (mimeType.StartsWith("text/"))
                    {
                        context.Response.ContentType += "; charset=utf-8";
                    }

                    await context.Response.Body.WriteAsync(data);

                    return;
                }
            }
        }

        await _next(context);
    }
}
