using InsightDocs.Extensions;
using InsightDocs.Model.DotNet;

namespace InsightDocs.Templates.MSDN;

public static class TemplateExtensions
{
    public static Builder UseMSDNTemplate(this Builder builder)
    {
        builder.RegisterRazorItemTemplate<DotNetIndex, DotNet.DotNetIndex>();
        builder.RegisterRazorItemTemplate<DotNetNamespace, DotNet.DotNetNamespace>();
        builder.RegisterRazorItemTemplate<DotNetType, DotNet.DotNetType>();

        return builder;
    }
}