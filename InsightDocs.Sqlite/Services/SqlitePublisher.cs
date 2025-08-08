using InsightDocs.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace InsightDocs.Sqlite.Services;

public class SqlitePublisherOptions
{
    public string? DatabasePath
    {
        get;
        set;
    }

    public bool ClearExistingDatabase
    {
        get;
        set;
    } = true;

    public bool GzipContent
    {
        get;
        set;
    } = false;
}

public class SqlitePublisher : IPublisher, IDisposable
{
    protected Dictionary<string, int> _mimeTypes = [];
    protected int _mimeTypeCounter = 1;

    public SqlitePublisher(SqlitePublisherOptions options, IServiceProvider serviceProvider)
    {
        string databasePath = options.DatabasePath ?? throw new ArgumentException("DatabasePath must be set in SqlitePublisherOptions.");

        if (!Path.IsPathRooted(databasePath))
        {
            databasePath = Path.Combine(AppContext.BaseDirectory, databasePath);
        }

        if (!Directory.Exists(Path.GetDirectoryName(databasePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        }

        DatabasePath = databasePath;
        ClearExistingDatabase = options.ClearExistingDatabase;
        GzipContent = options.GzipContent;
        SearchService = serviceProvider.GetService<ISearchService>();
    }

    protected bool ClearExistingDatabase
    {
        get;
        set;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal static string DatabasePath
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    protected bool GzipContent
    {
        get;
        set;
    }

    protected ISearchService? SearchService
    {
        get;
        set;
    }

    protected object _connectionLock = new object();

    internal static SqliteConnection? _connection;
    internal static SqliteTransaction? _transaction;

    protected ConcurrentDictionary<string, bool> PublishedUrls
    {
        get;
    } = new ConcurrentDictionary<string, bool>();

    internal static SqliteTransaction Transaction
    {
        get
        {
            _transaction ??= Connection.BeginTransaction();
            return _transaction;
        }
    }

    internal static SqliteConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection($"Data Source={DatabasePath}");
                _connection.Open();

                using (SqliteCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA journal_mode=WAL;";
                    command.ExecuteNonQuery();
                }
            }

            return _connection;
        }
    }

    public void Dispose()
    {
        if (_transaction != null)
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        using (SqliteConnection connection = new SqliteConnection($"Data Source={DatabasePath}"))
        {
            connection.Open();
        }
    }

    public async Task Initialize()
    {
        if (ClearExistingDatabase)
        {
            if (File.Exists(DatabasePath))
            {
                File.SetAttributes(DatabasePath, FileAttributes.Normal);
                File.Delete(DatabasePath);
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetName().Name + ".Resources.CreateDatabase.sql";
            Stream resourceStream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Unable to load the resource stream for {resourceName}.");

            using (resourceStream)
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await resourceStream.CopyToAsync(memoryStream);
                string createDatabaseScript = Encoding.UTF8.GetString(memoryStream.ToArray());

                using (SqliteConnection connection = new SqliteConnection($"Data Source={DatabasePath}"))
                {
                    connection.Open();

                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = createDatabaseScript;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }

    public async Task Publish(string url, object contents, string mimeType, string? title = null)
    {
        if (contents is not byte[] contentBytes)
        {
            contentBytes = contents is string contentString
                ? Encoding.UTF8.GetBytes(contentString)
                : throw new ArgumentException("Contents must be a byte array or a string.", nameof(contents));
        }

        if (GzipContent)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(contentBytes, 0, contentBytes.Length);
                }

                contentBytes = memoryStream.ToArray();
            }
        }

        lock (_connectionLock)
        {
            if (PublishedUrls.ContainsKey(url))
            {
                throw new Exception($"The URL {url} has already been published. Each URL must be unique.");
            }

            PublishedUrls.AddOrUpdate(url, true, (key, oldValue) => true);

            if (!_mimeTypes.TryGetValue(mimeType, out int mimeTypeId))
            {
                mimeTypeId = _mimeTypeCounter++;
                _mimeTypes[mimeType] = mimeTypeId;

                using (SqliteCommand command = new SqliteCommand("INSERT INTO MimeTypes (MimeTypeId, MimeType) VALUES(@id, @mimeType)", Connection, Transaction))
                {
                    command.Parameters.AddWithValue("@id", mimeTypeId);
                    command.Parameters.AddWithValue("@mimeType", mimeType);
                    command.ExecuteNonQuery();
                }
            }

            using (SqliteCommand command = new SqliteCommand("INSERT INTO Urls VALUES(@url, @mimeTypeId, @dataIsGzipped, @dataContentLength, @data)", Connection, Transaction))
            {
                if (!url.StartsWith('/'))
                {
                    url = '/' + url;
                }

                command.Parameters.AddWithValue("@url", url);
                command.Parameters.AddWithValue("@mimeTypeId", mimeTypeId);
                command.Parameters.AddWithValue("@dataIsGzipped", GzipContent);
                command.Parameters.AddWithValue("@dataContentLength", contentBytes.Length);
                command.Parameters.AddWithValue("@data", contentBytes);

                command.ExecuteNonQuery();
            }

            if (SearchService != null && SearchService is SqliteSearchService && mimeType == "text/html")
            {
                if (contents is string contentString)
                {
                    SearchService.IndexContentForSearch(url, contentString, title!);
                }

                else
                {
                    SearchService.IndexContentForSearch(url, Encoding.UTF8.GetString(contentBytes), title!);
                }
            }
        }

        if (SearchService != null && SearchService is not SqliteSearchService && mimeType == "text/html")
        {
            if (contents is string contentString)
            {
                await SearchService.IndexContentForSearch(url, contentString, title!);
            }

            else
            {
                await SearchService.IndexContentForSearch(url, Encoding.UTF8.GetString(contentBytes), title!);
            }
        }
    }

    public Task<bool> UrlWasPublished(string url)
    {
        return Task.FromResult(PublishedUrls.ContainsKey(url));
    }
}
