using InsightDocs.Abstractions;
using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Model;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.DotNet.Services;

public class XmlDocUrlResolver(IServiceProvider serviceProvider) : IXmlDocUrlResolver
{
    protected readonly Dictionary<string, string> Urls = [];
    protected readonly Dictionary<string, string> LinkTexts = [];
    protected IServiceProvider _serviceProvider = serviceProvider;

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
                        _serviceProvider.GetRequiredService<IDotNetLoader>().LoadType(type);
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
                Console.WriteLine("Warning: No XmlDoc target registered for key " + key);
                // TODO
                //throw new Exception("No XmlDoc target registered for key: " + key + ".");
                value = "";
            }
        }

        return value;
    }

    public string GetLinkText(string key)
    {
        if (!LinkTexts.TryGetValue(key, out string? value))
        {
            Console.WriteLine("Warning: No XmlDoc target registered for key " + key);
            return "";
            // TODO
            // throw new Exception("No XmlDoc target registered for key: " + key + ".");
        }

        return value;
    }

    public void RegisterLookup<T>(T target, string key) where T : ILinkTarget
    {
        if (Urls.ContainsKey(key))
        {
            throw new Exception("XmlDoc target already registered for key: " + key + ".");
        }

        IUrlProvider<T> urlProvider = _serviceProvider.GetService<IUrlProvider<T>>() ?? throw new Exception("No IUrlProvider service registered for " + typeof(T) + ".");
        Urls[key] = urlProvider.GetUrl(target);
        LinkTexts[key] = target.LinkText;
    }
}