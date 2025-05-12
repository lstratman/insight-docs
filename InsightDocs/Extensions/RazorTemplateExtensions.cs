using System.Text;
using InsightDocs.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InsightDocs.Extensions;

public class RazorTemplateRenderer<T, TTemplate>(IServiceProvider serviceProvider) : IItemTemplateProvider<T> where TTemplate: IComponent
{
    protected IServiceProvider _serviceProvider = serviceProvider;

    public async Task<byte[]> GetContent(T item)
    {
        ILoggerFactory loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        using HtmlRenderer htmlRenderer = new(_serviceProvider, loggerFactory);
        
        return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "Item", item },
                { "ServiceProvider", _serviceProvider }
            };

            ParameterView parameters = ParameterView.FromDictionary(dictionary);
            HtmlRootComponent output = await htmlRenderer.RenderComponentAsync<TTemplate>(parameters);

            return Encoding.UTF8.GetBytes(output.ToHtmlString());
        });
    }
}

public static class RazorTemplateExtensions
{
    public static void RegisterRazorItemTemplate<T, TTemplate>(this Builder builder) where TTemplate : IComponent
    {
        builder.Services.AddSingleton<IItemTemplateProvider<T>>((serviceProvider) => {
            return new RazorTemplateRenderer<T, TTemplate>(serviceProvider);
        });
    }
}