using InsightDocs.TypeScript;
using Microsoft.AspNetCore.Components;

namespace InsightDocs.Templates.MicrosoftLearn.TypeScript
{
    public class TypeScriptMethodBase : RazorTypeScriptTemplate<InsightDocs.TypeScript.Model.TypeScriptMethod>
    {
        public virtual RenderFragment AddAdditionalMethodSignatureProperties(InsightDocs.TypeScript.Model.TypeScriptMethodSignature typeScriptMethodSignature)
        {
            return (builder) =>
            {
            };
        }
    }
}
