using InsightDocs.Abstractions;
using InsightDocs.DotNet.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InsightDocs.DotNet.Services;

public partial class XmlDocUrlResolver(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : IXmlDocUrlResolver
{
    protected readonly Dictionary<string, string> Urls = [];
    protected readonly Dictionary<string, string> LinkTexts = [];
    protected ILogger logger = loggerFactory.CreateLogger<XmlDocUrlResolver>();

    [LoggerMessage(LogLevel.Warning, "No XmlDoc target registered for key {key}")]
    public static partial void LogNoXmlDocTargetRegistered(ILogger logger, string key);

    public string GetUrl(string key)
    {
        if (!Urls.TryGetValue(key, out string? value))
        {
            if (key.StartsWith("T:"))
            {
                try
                {
                    Type? type = Type.GetType(key[2..]);

                    if (type != null)
                    {
                        serviceProvider.GetRequiredService<IDotNetLoader>().LoadType(type);
                        Urls.TryGetValue(key, out string? value2);
                        value = value2;
                    }
                }

                catch (Exception)
                {
                }
            }

            if (value == null)
            {
                // TODO: customizable behavior
                LogNoXmlDocTargetRegistered(logger, key);
                value = "";
            }
        }

        return value;
    }

    public string GetLinkText(string key)
    {
        if (!LinkTexts.TryGetValue(key, out string? value))
        {
            // TODO: customizable behavior
            LogNoXmlDocTargetRegistered(logger, key);
            return "";
        }

        return value;
    }

    public void RegisterLookup<T>(T target, string key) where T : ILinkTarget
    {
        if (Urls.ContainsKey(key))
        {
            throw new Exception("XmlDoc target already registered for key: " + key + ".");
        }

        IUrlProvider<T> urlProvider = serviceProvider.GetService<IUrlProvider<T>>() ?? throw new Exception("No IUrlProvider service registered for " + typeof(T) + ".");
        Urls[key] = urlProvider.GetUrl(target);
        LinkTexts[key] = target.LinkText;
    }
}