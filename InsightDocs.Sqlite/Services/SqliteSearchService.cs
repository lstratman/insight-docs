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

public static class SearchUtil
{
    public const int TOKEN_TYPE_WHITESPACE = 1;
    public const int TOKEN_TYPE_WORD = 2;
    public const int TOKEN_TYPE_SYMBOL = 3;

    public static Regex WordDelimiterRegex = new Regex(@"[\s\:\,\;\+\-\'\""\(\)\{\}\[\]\/\\\\\u200B]+");

    public static IEnumerable<(int position, int length, int type)> Tokenize(ReadOnlySpan<char> input)
    {
        int p = 0;
        int q;
        int len = input.Length;
        List<(int position, int length, int type)> list = [];

        while (p < len)
        {
            if (Char.IsWhiteSpace(input[p]))
            {
                q = p + 1;

                while (q < len && Char.IsWhiteSpace(input[q]))
                {
                    q++;
                }

                list.Add((p, q - p, 1));
                p = q;
            }

            else if (Char.IsLetterOrDigit(input[p]))
            {
                q = p + 1;

                while (q < len && Char.IsLetterOrDigit(input[q]))
                {
                    q++;
                }

                list.Add((p, q - p, 2));
                p = q;
            }

            else
            {
                list.Add((p, 1, 3));
                p++;
            }
        }

        return list;
    }

    public static List<string> SplitPhrase(string phrase)
    {
        return [.. Tokenize(phrase.AsSpan()).Where(x => x.type % 2 == 0).Select(x => phrase.Substring(x.position, x.length))];
    }

    public static readonly HashSet<string> StopWords =
    [
        "a", "all", "an", "and", "any", "are", "as", "at",
        "be", "but", "by",
        "can",
        "did", "do",
        "for",
        "get", "got",
        "had", "has", "have", "he", "her", "him", "his", "how",
        "if", "in", "is", "it", "its",
        "me", "my",
        "now",
        "of", "on", "or", "our",
        "she", "so",
        "the", "to",
        "up",
        "was", "were", "with",
    ];

    public static readonly HashSet<string> ImportantWords = ["no", "not", "without", "right", "left"];

    /// <summary>
    /// Case-insensitive check if the given word is in the list of <see cref="StopWords"/>
    /// </summary>
    /// <param name="word"></param>
    public static bool IsStopWord(string word)
    {
        return StopWords.Contains(word.ToLowerInvariant());
    }

    public static bool IsSearchWord(string word)
    {
        return word.Length > 1 && Char.IsLetter(word[0]) && !IsStopWord(word);
    }

    /// <summary>
    /// True if the given word is in the list of lowercase <see cref="StopWords"/>, use when 
    /// you know the input word is already in lower case
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    public static bool IsLowerCaseStopWord(string word)
    {
        return StopWords.Contains(word);
    }

    private static readonly Func<string, double> EmptyScoringFunction = x => 0.0;

    private class WordInfo
    {
        public required string Word
        {
            get;
            set;
        }

        public double Weight
        {
            get;
            set;
        }

        public double Score
        {
            get;
            set;
        }

        public int Length
        {
            get;
            set;
        }

        public int Ordinal
        {
            get;
            set;
        }

        public bool IsStop
        {
            get;
            set;
        }

        public double Size
        {
            get;
            set;
        }
    }

    private static double WordPositionScore(int position)
    {
        return (1.0 + (1.0 / (position + 1))) / 2.0;
    }

    private static List<WordInfo> GetWordInfo(string phrase)
    {
        List<WordInfo> list = [];
        int totalLength = 0;
        int wordCount = 0;
        double stopCount = 0;
        int ordinal = 0;
        double totalScore = 0.0;
        double totalSize = 0.0;

        foreach (string word in SplitPhrase(phrase))
        {
            WordInfo info = new WordInfo
            {
                Word = word,
                Ordinal = ordinal,
                Length = word.Length,
                IsStop = IsStopWord(word),
                Size = Math.Min(Math.Ceiling(word.Length / 3.0), 5.0)
            };

            if (ImportantWords.Contains(word))
            {
                info.Size = 5.0;
            }

            totalLength += word.Length;
            totalSize += info.Size;
            wordCount++;
            stopCount += info.IsStop ? 1 : 0;
            ordinal++;
            list.Add(info);
        }

        int scoreFactors = 2;
        double stopAdjustment = 0.0;

        if (stopCount > 0 && stopCount < wordCount)
        {
            stopAdjustment = 1.0 / (wordCount - stopCount);
            scoreFactors++;
        }

        foreach (WordInfo info in list)
        {
            double sizeScore = info.Size / totalSize;
            double distScore = WordPositionScore(info.Ordinal);
            double stopScore = stopAdjustment > 0 && !info.IsStop ? stopAdjustment : 0.0;

            info.Score = (sizeScore + distScore + stopScore) / scoreFactors;
            totalScore += info.Score;
        }

        foreach (WordInfo info in list)
        {
            info.Weight = info.Score / totalScore;
        }

        return list;
    }

    public static int LCP(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        int i = 0;
        int length = Math.Min(a.Length, b.Length);

        while (i < length && a[i] == b[i])
        {
            i++;
        }

        return i;
    }

    private static double ComparePhrases(List<WordInfo> A, List<WordInfo> B)
    {
        if (A.Count > B.Count)
        {
            (B, A) = (A, B);
        }

        double totalPoints = 0;
        bool[] usedA = new bool[A.Count];
        bool[] usedB = new bool[B.Count];
        List<(int a, int b, double score)> scores = [];

        int i = 0;
        int j = 0;

        while (i < A.Count)
        {
            int moveCount = B.Count;

            while (moveCount > 0)
            {
                if (!usedB[j])
                {
                    double score = 2.0 * LCP(A[i].Word.AsSpan(), B[j].Word.AsSpan()) / (A[i].Length + B[j].Length);

                    if (score > 0)
                    {
                        if (score >= 1)
                        {
                            totalPoints += score * (A[i].Weight + B[j].Weight);
                            usedA[i] = true;
                            usedB[j] = true;
                            j = (j + 1) % B.Count;

                            break;
                        }

                        else
                        {
                            scores.Add((i, j, score));
                        }
                    }
                }

                j = (j + 1) % B.Count;
                moveCount--;
            }

            i++;
        }

        foreach ((int a, int b, double score) in scores.OrderByDescending(x => x.score))
        {
            if (!usedA[a] && !usedB[b])
            {
                totalPoints += score * (A[a].Weight + B[b].Weight);
                usedA[a] = true;
                usedB[b] = true;
            }
        }

        return totalPoints / 2.0;
    }

    public static Func<string, double> CreateScoringFunction(string query, Func<string, string>? normalizer = null)
    {
        normalizer ??= x => x;

        string normalizedQuery = normalizer(query);
        List<WordInfo> sourceWords = GetWordInfo(normalizedQuery);

        if (sourceWords.Count == 0)
        {
            return EmptyScoringFunction;
        }

        double CalcScore(string target)
        {
            string normTarget = normalizer(target);

            if (normTarget == normalizedQuery)
            {
                return 1.0;
            }

            List<WordInfo> targetWords = GetWordInfo(normTarget);
            return ComparePhrases(sourceWords, targetWords);
        }

        return CalcScore;
    }

    public static Func<string, string> CreateNormalizer(bool lowerCase = true, bool useStandardStopWords = true, List<string>? additionalStopWords = null, Regex? precleanExpr = null)
    {
        Func<string, string> preProcess = x => x;

        if (lowerCase)
        {
            Func<string, string> currentProcessor = preProcess;
            preProcess = x => currentProcessor(x).ToLower();
        }

        if (precleanExpr != null)
        {
            Func<string, string> currentProcessor = preProcess;
            preProcess = x => precleanExpr.Replace(currentProcessor(x), "");
        }

        Func<string, bool> filter = x => true;

        if (useStandardStopWords)
        {
            Func<string, bool> currentProcessor = filter;
            filter = x => currentProcessor(x) && !IsStopWord(x);
        }

        if (additionalStopWords != null)
        {
            Func<string, bool> currentProcessor = filter;
            Regex additionalStopWordsRegex = new Regex("^(" + String.Join("|", additionalStopWords) + ")$", RegexOptions.IgnoreCase);
            filter = x => currentProcessor(x) && !additionalStopWordsRegex.IsMatch(x);
        }

        return input => String.Join(" ", SplitPhrase(preProcess(input)).Where(filter));

    }
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

    public virtual Func<string, double> CreateTitleScoringFunction(string query)
    {
        return SearchUtil.CreateScoringFunction(query, SearchUtil.CreateNormalizer());
    }

    public virtual int ScoreSearchResults(string offsets, string query, string title, Func<string, double> titleScoringFunction)
    {
        int score = 0;
        int actualLength = query.Length;
        int directMatch = 1;
        bool titleMatched = false;

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
                if (!titleMatched)
                {
                    double titleScore = titleScoringFunction(title);

                    score += Convert.ToInt32(5000 * (1 + titleScore));
                    titleMatched = true;
                }
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

    public virtual Task<List<JsonSearchResult>> ExecuteSearch(string query, Func<string, string, bool>? additionalFilter = null)
    {
        SqliteMiddlewareOptions sqliteMiddlewareOptions = serviceProvider.GetRequiredService<SqliteMiddlewareOptions>();

        if (String.IsNullOrEmpty(sqliteMiddlewareOptions.DatabasePath))
        {
            throw new InvalidOperationException("DatabasePath must be set in SqliteMiddlewareOptions.");
        }

        using (SqliteConnection connection = new SqliteConnection($"Data Source={sqliteMiddlewareOptions.DatabasePath};Mode=ReadOnly"))
        {
            connection.Open();

            Func<string, double> titleScoringFunction = CreateTitleScoringFunction(query);

            connection.CreateFunction("score", (string offsets, string query, string title) =>
            {
                return ScoreSearchResults(offsets, query, title, titleScoringFunction);
            });

            if (additionalFilter != null)
            {
                connection.CreateFunction("extraFilter", additionalFilter);
            }

            using (SqliteCommand command = new SqliteCommand($@"SELECT Url, snippet(Search, '<b>', '</b>', '...', 1), snippet(Search, '<b>', '</b>', '...', 2), score(offsets(Search), @query, Title) AS score
                                                                FROM Search 
                                                                WHERE Search MATCH @queryFlex {(additionalFilter == null ? "" : " AND extraFilter(Url, Title)")}
                                                                ORDER BY score DESC
                                                                LIMIT " + options.SearchResultsLimit, connection))
            {
                command.Parameters.AddWithValue("@query", query);
                command.Parameters.AddWithValue("@queryFlex", query.Replace("  ", " ").Trim().Replace(" ", "* OR ") + "*");

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    List<JsonSearchResult> results = [];

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
