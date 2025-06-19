using InsightDocs.Abstractions;
using Microsoft.Extensions.Logging;

namespace InsightDocs.Services;

public partial class CodeLanguageService(InsightDocsOptions options, ILoggerFactory loggerFactory) : ICodeLanguageService
{
    protected ILogger logger = loggerFactory.CreateLogger<CodeLanguageService>();

    [LoggerMessage(LogLevel.Warning, "Unknown code language: {languageId}")]
    public static partial void LogUnknownCodeLanguage(ILogger logger, string languageId);

    public string GetTitle(string languageId)
    {
        if (languageId == "csharp" || languageId == "cs" || languageId == "c#")
        {
            return "C#";
        }

        else if (languageId == "vb" || languageId == "visualbasic")
        {
            return "Visual Basic";
        }

        else if (languageId == "javascript" || languageId == "js" || languageId == "typescript" || languageId == "ts")
        {
            return "JavaScript/TypeScript";
        }

        else if (languageId == "xml")
        {
            return "XML";
        }

        else if (languageId == "html")
        {
            return "HTML";
        }

        else if (languageId == "json")
        {
            return "JSON";
        }

        else if (languageId == "ini")
        {
            return "Configuration";
        }

        else if (languageId == "terminal")
        {
            return "Terminal";
        }

        else if (languageId == "sql")
        {
            return "SQL";
        }

        else if (languageId == "yml")
        {
            return "YAML";
        }

        else if (languageId == "dockerfile" || languageId == "docker")
        {
            return "Docker";
        }

        else
        {
            if (options.UnknownCodeLanguageBehavior == ErrorBehavior.Error)
            {
                throw new ArgumentException($"Unknown code language: {languageId}");
            }

            else if (options.UnknownCodeLanguageBehavior == ErrorBehavior.Warn)
            {
                LogUnknownCodeLanguage(logger, languageId);
            }

            return "";
        }
    }
}
