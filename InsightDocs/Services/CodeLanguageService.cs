using InsightDocs.Abstractions;
using Microsoft.Extensions.Logging;

namespace InsightDocs.Services;

public partial class CodeLanguageService(
    InsightDocsOptions options,
    ILoggerFactory loggerFactory) : ICodeLanguageService
{
    protected ILogger logger = loggerFactory.CreateLogger<CodeLanguageService>();

    [LoggerMessage(LogLevel.Warning, "Unknown code language: {languageId}")]
    public static partial void LogUnknownCodeLanguage(ILogger logger, string languageId);

    public virtual string GetTitle(string languageId)
    {
        if (languageId is "csharp" or "cs" or "c#")
        {
            return "C#";
        }

        else if (languageId is "vb" or "visualbasic")
        {
            return "Visual Basic";
        }

        else if (languageId is "javascript" or "js")
        {
            return "JavaScript";
        }

        else if (languageId is "typescript" or "ts")
        {
            return "TypeScript";
        }

        else if (languageId == "json")
        {
            return "JSON";
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
