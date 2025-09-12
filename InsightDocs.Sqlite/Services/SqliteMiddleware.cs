using InsightDocs.Abstractions;
using InsightDocs.Site.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace InsightDocs.Sqlite.Services;

public class SqliteMiddlewareOptions
{
    public string? DatabasePath
    {
        get;
        set;
    }

    public bool UseDynamicToc
    {
        get;
        set;
    } = false;

    public string DynamicTocUrl
    {
        get;
        set;
    } = "/_toc";
}

public class SqliteMiddleware
{
    protected readonly RequestDelegate _next;
    protected SiteToc? _siteToc = null;

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

    protected bool UseDynamicToc
    {
        get;
        set;
    }

    protected string DynamicTocUrl
    {
        get;
        set;
    }

    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public SqliteMiddleware(RequestDelegate next, IHostingEnvironment env, SqliteMiddlewareOptions options, IServiceProvider serviceProvider)
    {
        _next = next;

        if (options.UseDynamicToc && String.IsNullOrEmpty(options.DynamicTocUrl))
        {
            throw new ArgumentException("DynamicTocUrl must be provided in SqliteMiddlewareOptions when UseDynamicToc is true.");
        }

        UseDynamicToc = options.UseDynamicToc;
        DynamicTocUrl = options.DynamicTocUrl;

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

    protected SiteToc TocData
    {
        get
        {
            if (_siteToc == null)
            {
                using (SqliteConnection connection = GetConnection())
                using (SqliteCommand command = new SqliteCommand("SELECT DataIsGzipped, Data FROM Urls WHERE url = '/table-of-contents.json'", connection))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool dataIsGzipped = reader.GetBoolean(0);
                            byte[] data = reader.GetFieldValue<byte[]>(1);

                            if (dataIsGzipped)
                            {
                                using (MemoryStream compressedStream = new MemoryStream(data))
                                using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                                using (MemoryStream decompressedStream = new MemoryStream())
                                {
                                    gzipStream.CopyTo(decompressedStream);
                                    data = decompressedStream.ToArray();
                                }
                            }

                            string tocJson = Encoding.UTF8.GetString(data);
                            _siteToc = JsonSerializer.Deserialize<SiteToc>(tocJson);
                        }

                        else
                        {
                            throw new Exception("Unable to load the table of contents from the database.");
                        }
                    }
                }
            }

            return _siteToc!;
        }
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

        else if (UseDynamicToc && path == $"{DynamicTocUrl}/root-items" && context.Request.Method == "GET")
        {
            await context.Response.WriteAsJsonAsync(TocData.RootItems.ToDictionary(key => key, key => TocData.Items[key]), SerializerOptions);
            return;
        }

        else if (UseDynamicToc && path == $"{DynamicTocUrl}/index" && context.Request.Method == "GET" && context.Request.Query.ContainsKey("url"))
        {
            string url = context.Request.Query["url"].ToString();

            if (TocData.UrlLookups.TryGetValue(url, out int value))
            {
                await context.Response.WriteAsJsonAsync(value, SerializerOptions);
            }

            else if (url.Contains('#') && TocData.UrlLookups.ContainsKey(url[..url.IndexOf('#')]))
            {
                await context.Response.WriteAsJsonAsync(TocData.UrlLookups[url[..url.IndexOf('#')]], SerializerOptions);
            }

            else
            {
                await context.Response.WriteAsJsonAsync(-1, SerializerOptions);
            }

            return;
        }

        else if (UseDynamicToc && context.Request.Path.StartsWithSegments($"{DynamicTocUrl}/children") && context.Request.Method == "GET")
        {
            string[] segments = context.Request.Path.Value!.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 3 && Int32.TryParse(segments[2], out int parentId) && parentId >= 0 && parentId < TocData.Items.Count)
            {
                List<int> childIndices = TocData.Items[parentId].ChildIndices ?? [];
                await context.Response.WriteAsJsonAsync(childIndices.ToDictionary(key => key, key => TocData.Items[key]), SerializerOptions);
                return;
            }
        }

        else if (UseDynamicToc && context.Request.Path.StartsWithSegments($"{DynamicTocUrl}/children") && context.Request.Method == "POST")
        {
            List<int> itemsToReturn = await context.Request.ReadFromJsonAsync<List<int>>(SerializerOptions) ?? [];
            List<int> childIndices = [.. itemsToReturn];

            foreach (int itemToReturn in itemsToReturn)
            {
                if (TocData.Items[itemToReturn].ChildIndices != null)
                {
                    childIndices.AddRange(TocData.Items[itemToReturn].ChildIndices!);
                }
            }

            await context.Response.WriteAsJsonAsync(childIndices.Distinct().ToDictionary(key => key, key => TocData.Items[key]), SerializerOptions);
            return;
        }

        else if (UseDynamicToc && context.Request.Path.StartsWithSegments($"{DynamicTocUrl}/ancestors") && context.Request.Method == "GET")
        {
            string[] segments = context.Request.Path.Value!.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 3 && Int32.TryParse(segments[2], out int parentId) && parentId >= 0 && parentId < TocData.Items.Count)
            {
                List<int> ancestors = [];

                while (parentId >= 0)
                {
                    ancestors.Add(parentId);
                    parentId = TocData.Items[parentId].ParentIndex ?? -1;
                }

                await context.Response.WriteAsJsonAsync(ancestors, SerializerOptions);
                return;
            }
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
                        context.Response.Headers.Append("Content-Encoding", "gzip");
                    }

                    await context.Response.Body.WriteAsync(data);

                    return;
                }
            }
        }

        await _next(context);
    }
}

internal static class JsonConstants
{
    public const string JsonContentType = "application/json";
    public const string JsonContentTypeWithCharset = "application/json; charset=utf-8";
}

/// <summary>
/// Provides extension methods for writing a JSON serialized value to the HTTP response.
/// </summary>
public static partial class HttpResponseJsonExtensions
{
    private const string RequiresUnreferencedCodeMessage = "JSON serialization and deserialization might require types that cannot be statically analyzed. " +
        "Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.";
    private const string RequiresDynamicCodeMessage = "JSON serialization and deserialization might require types that cannot be statically analyzed and need runtime code generation. " +
        "Use the overload that takes a JsonTypeInfo or JsonSerializerContext for native AOT applications.";

    /// <summary>
    /// Read JSON from the request and deserialize to the specified type.
    /// If the request's content-type is not a known JSON type then an error will be thrown.
    /// </summary>
    /// <typeparam name="TValue">The type of object to read.</typeparam>
    /// <param name="request">The request to read from.</param>
    /// <param name="options">The serializer options to use when deserializing the content.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static ValueTask<TValue?> ReadFromJsonAsync<TValue>(
        this HttpRequest request,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.ReadFromJsonAsync(jsonTypeInfo: (JsonTypeInfo<TValue>)options.GetTypeInfo(typeof(TValue)), cancellationToken);
    }

    private static Encoding? GetEncodingFromCharset(StringSegment charset)
    {
        if (charset.Equals("utf-8", StringComparison.OrdinalIgnoreCase))
        {
            // This is an optimization for utf-8 that prevents the Substring caused by
            // charset.Value
            return Encoding.UTF8;
        }

        try
        {
            // charset.Value might be an invalid encoding name as in charset=invalid.
            return charset.HasValue ? Encoding.GetEncoding(charset.Value) : null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unable to read the request as JSON because the request content type charset '{charset}' is not a known encoding.", ex);
        }
    }

    private static (Stream inputStream, bool usesTranscodingStream) GetInputStream(HttpContext httpContext, Encoding? encoding)
    {
        if (encoding == null || encoding.CodePage == Encoding.UTF8.CodePage)
        {
            return (httpContext.Request.Body, false);
        }

        Stream inputStream = Encoding.CreateTranscodingStream(httpContext.Request.Body, encoding, Encoding.UTF8, leaveOpen: true);
        return (inputStream, true);
    }

    private static bool HasJsonContentType(this HttpRequest request, out StringSegment charset)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out MediaTypeHeaderValue? mt))
        {
            charset = StringSegment.Empty;
            return false;
        }

        // Matches application/json
        if (mt.MediaType.Equals(JsonConstants.JsonContentType, StringComparison.OrdinalIgnoreCase))
        {
            charset = mt.Charset;
            return true;
        }

        // Matches +json, e.g. application/ld+json
        if (mt.Suffix.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            charset = mt.Charset;
            return true;
        }

        charset = StringSegment.Empty;
        return false;
    }

    [DoesNotReturn]
    private static void ThrowContentTypeError(HttpRequest request)
    {
        throw new InvalidOperationException($"Unable to read the request as JSON because the request content type '{request.ContentType}' is not a known JSON content type.");
    }

    /// <summary>
    /// Read JSON from the request and deserialize to the specified type.
    /// If the request's content-type is not a known JSON type then an error will be thrown.
    /// </summary>
    /// <param name="request">The request to read from.</param>
    /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The deserialized value.</returns>
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
    public static async ValueTask<TValue?> ReadFromJsonAsync<TValue>(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
        this HttpRequest request,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.HasJsonContentType(out StringSegment charset))
        {
            ThrowContentTypeError(request);
        }

        Encoding? encoding = GetEncodingFromCharset(charset);
        (Stream inputStream, bool usesTranscodingStream) = GetInputStream(request.HttpContext, encoding);

        try
        {
            return await JsonSerializer.DeserializeAsync(inputStream, jsonTypeInfo, cancellationToken);
        }

        finally
        {
            if (usesTranscodingStream)
            {
                await inputStream.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// <c>application/json; charset=utf-8</c>.
    /// </summary>
    /// <typeparam name="TValue">The type of object to write.</typeparam>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="options">The serializer options to use when serializing the value.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    public static Task WriteAsJsonAsync<TValue>(
        this HttpResponse response,
        TValue value,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        return response.WriteAsJsonAsync(value, options, contentType: null, cancellationToken);
    }

    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// the specified content-type.
    /// </summary>
    /// <typeparam name="TValue">The type of object to write.</typeparam>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="options">The serializer options to use when serializing the value.</param>
    /// <param name="contentType">The content-type to set on the response.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    public static Task WriteAsJsonAsync<TValue>(
        this HttpResponse response,
        TValue value,
        JsonSerializerOptions options,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.ContentType = contentType ?? JsonConstants.JsonContentTypeWithCharset;

        // if no user provided token, pass the RequestAborted token and ignore OperationCanceledException
#pragma warning disable IDE0046 // Convert to conditional expression
        if (!cancellationToken.CanBeCanceled)
        {
            return WriteAsJsonAsyncSlow(response.Body, value, options, response.HttpContext.RequestAborted);
        }
#pragma warning restore IDE0046 // Convert to conditional expression

        return JsonSerializer.SerializeAsync(response.Body, value, options, cancellationToken);
    }

    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    private static async Task WriteAsJsonAsyncSlow<TValue>(
        Stream body,
        TValue value,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken)
    {
        try
        {
            await JsonSerializer.SerializeAsync(body, value, options, cancellationToken);
        }
        catch (OperationCanceledException) { }
    }
}