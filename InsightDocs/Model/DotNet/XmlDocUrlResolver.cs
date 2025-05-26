using InsightDocs.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.Model.DotNet;

public class XmlDocUrlResolver
{
    private static Dictionary<string, string> Urls = [];
    private static Dictionary<string, string> LinkTexts = [];
    private static IServiceProvider? ServiceProvider = null;

    public static string GetUrl(string key)
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
                        DotNetType.Resolve(type);
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

    public static string GetLinkText(string key)
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

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public static void RegisterLookup<T>(T target, string key) where T : ILinkTarget
    {
        if (Urls.ContainsKey(key))
        {
            throw new Exception("XmlDoc target already registered for key: " + key + ".");
        }

        if (ServiceProvider == null)
        {
            throw new Exception("ServiceProvider has not been set");
        }

        IUrlProvider<T> urlProvider = ServiceProvider.GetService<IUrlProvider<T>>() ?? throw new Exception("No IUrlProvider service registered for " + typeof(T) + ".");
        Urls[key] = urlProvider.GetUrl(target);
        LinkTexts[key] = target.LinkText;
    }
}