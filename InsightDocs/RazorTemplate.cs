using InsightDocs.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs;

public class RazorTemplate
{
    public static RenderFragment GetLink<T>(T item, string text, IServiceProvider serviceProvider)
    {
        IUrlProvider<T> urlProvider = serviceProvider.GetService<IUrlProvider<T>>() ?? throw new Exception("No IUrlProvider service registered for " + typeof(T).Name + ".");
        string url = urlProvider.GetUrl(item);

        return (builder) => {
            builder.AddMarkupContent(0, $@"<a href=""{url}"">{text}</a>");
        };
    }
}