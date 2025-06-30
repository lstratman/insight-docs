using InsightDocs.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NUglify;
using System.Text.RegularExpressions;

namespace InsightDocs.Sqlite.Services;

public class SqliteSearchServiceOptions
{
    public string SearchUrl 
    { 
        get; 
        set; 
    } = "/_search";

    public int SearchResultsLimit
    {
        get;
        set;
    } = 12;
}

public class SqliteSearchService(SqliteSearchServiceOptions options, IServiceProvider serviceProvider) : ISearchService, ILocalSearchService
{
    protected static Regex buttonTagRegex = new Regex(@"<button(.*?)</button>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    protected static Regex preCodeRegex = new Regex(@"<pre><code>(.*?)</code></pre>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    protected static Regex offsetData = new Regex(@"(?<columnNumber>\d+) (?<termNumber>\d+) (?<matchOffset>\d+) (?<matchSize>\d+)");

    public virtual string SearchUrl
    {
        get
        {
            return options.SearchUrl;
        }
    }

    public virtual int ScoreSearchResults(string offsets, string query)
    {
        int score = 0;
        int actualLength = query.Length;
        int directMatch = 1;

        foreach (Match match in offsetData.Matches(offsets))
        {
            int columnNumber = Convert.ToInt32(match.Groups["columnNumber"].Value);

            if (columnNumber == 0)
            {
                continue;
            }

            int termNumber = Convert.ToInt32(match.Groups["termNumber"].Value);
            int matchOffset = Convert.ToInt32(match.Groups["matchOffset"].Value);
            int matchSize = Convert.ToInt32(match.Groups["matchSize"].Value);

            if (matchSize == actualLength)
            {
                directMatch = 2;
            }

            if (columnNumber == 1) // title
            {
                score += 50 * directMatch;
            }

            else
            {
                score += 1 * directMatch;
            }
        }

        if (directMatch == 2)
        {
            score += 12;
        }

        return score;
    }

    public virtual Task<List<JsonSearchResult>> ExecuteSearch(string query)
    {
        SqliteMiddlewareOptions sqliteMiddlewareOptions = serviceProvider.GetRequiredService<SqliteMiddlewareOptions>();

        if (String.IsNullOrEmpty(sqliteMiddlewareOptions.DatabasePath))
        {
            throw new InvalidOperationException("DatabasePath must be set in SqliteMiddlewareOptions.");
        }

        using (SqliteConnection connection = new SqliteConnection($"Data Source={sqliteMiddlewareOptions.DatabasePath};Mode=ReadOnly"))
        {
            connection.Open();

            connection.CreateFunction("score", (string offsets, string query) =>
            {
                return ScoreSearchResults(offsets, query);
            });

            using (SqliteCommand command = new SqliteCommand(@"SELECT Url, snippet(Search, '<b>', '</b>', '...', 1), snippet(Search, '<b>', '</b>', '...', 2), score(offsets(Search), @query) AS score
                                                               FROM Search 
                                                               WHERE Search MATCH @queryFlex 
                                                               ORDER BY score DESC
                                                               LIMIT " + options.SearchResultsLimit, connection))
            {
                command.Parameters.AddWithValue("@query", query);
                command.Parameters.AddWithValue("@queryFlex", query.Replace("  ", " ").Trim().Replace(" ", "* OR ") + "*");

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    List<JsonSearchResult> results = new List<JsonSearchResult>();

                    while (reader.Read())
                    {
                        string url = reader.GetString(0);
                        string title = reader.GetString(1);
                        string content = reader.GetString(2);

                        results.Add(new JsonSearchResult
                        {
                            Url = url,
                            ResultSummaryHtml = $"<h2>{title}</h2><p>{content}</p>"
                        });
                    }

                    return Task.FromResult(results);
                }
            }
        }
    }

    public virtual Task IndexContentForSearch(string url, string content, string title)
    {
        using (SqliteCommand command = new SqliteCommand("INSERT INTO Search VALUES(@url, @title, @content)", SqlitePublisher.Connection, SqlitePublisher.Transaction))
        {
            if (!url.StartsWith('/'))
            {
                url = '/' + url;
            }

            content = buttonTagRegex.Replace(content, "");
            content = preCodeRegex.Replace(content, "");

            UglifyResult uglifiedHtmlResult = Uglify.HtmlToText(content);

            if (String.IsNullOrEmpty(uglifiedHtmlResult.Code) && uglifiedHtmlResult.HasErrors)
            {
                throw new Exception($"Error uglifying HTML content: {String.Join(", ", uglifiedHtmlResult.Errors.Select(e => e.Message))}");
            }

            string uglifiedHtml = uglifiedHtmlResult.Code;

            command.Parameters.AddWithValue("@url", url);
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@content", uglifiedHtml);

            command.ExecuteNonQuery();
        }

        return Task.CompletedTask;
    }
}
