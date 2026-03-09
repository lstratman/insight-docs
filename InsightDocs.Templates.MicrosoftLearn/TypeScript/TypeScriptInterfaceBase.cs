using InsightDocs.TypeScript;
using Microsoft.AspNetCore.Components;

namespace InsightDocs.Templates.MicrosoftLearn.TypeScript
{
    public class TypeScriptInterfaceBase : RazorTypeScriptTemplate<InsightDocs.TypeScript.Model.TypeScriptInterface>
    {
        public virtual RenderFragment AddAdditionalInterfaceProperties(InsightDocs.TypeScript.Model.TypeScriptInterface typeScriptInterface)
        {
            return (builder) =>
            {
            };
        }
    }
}
