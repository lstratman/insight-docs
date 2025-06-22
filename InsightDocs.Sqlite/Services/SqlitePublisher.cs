using InsightDocs.Abstractions;
using Microsoft.Data.Sqlite;
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
}

public class SqlitePublisher : IPublisher, IDisposable
{
    public SqlitePublisher(SqlitePublisherOptions options)
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
    }

    protected bool ClearExistingDatabase
    {
        get;
        set;
    }

    protected string DatabasePath
    {
        get;
        set;
    }

    protected object _connectionLock = new object();
    protected SqliteConnection? _connection;
    protected SqliteTransaction? _transaction;

    protected SqliteTransaction Transaction
    {
        get
        {
            _transaction ??= Connection.BeginTransaction();
            return _transaction;
        }
    }

    protected SqliteConnection Connection
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

    public Task Publish(string url, byte[] contents)
    {
        lock (_connectionLock)
        {
            using (SqliteCommand command = new SqliteCommand("INSERT INTO Topics VALUES(@url, @data)", Connection, Transaction))
            {
                command.Parameters.AddWithValue("@url", url);
                command.Parameters.AddWithValue("@data", contents);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }
    }
}
