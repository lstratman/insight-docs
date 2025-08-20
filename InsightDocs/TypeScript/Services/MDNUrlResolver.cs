using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Model.Types;

namespace InsightDocs.TypeScript.Services
{
    public class MDNUrlResolver : IMDNUrlResolver
    {
        public string GetUrl(TypeScriptTypeDeclaration item)
        {
            string typeName = item.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}";
            }

            else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}";
            }

            // TODO: throw error
            return "";
        }

        public string GetUrl(TypeScriptInterface item)
        {
            string typeName = item.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}";
            }

            else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}";
            }

            // TODO: throw error
            return "";
        }

        public string GetUrl(TypeScriptMethod item)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];
            string typeName = sourceType.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}/{item.Name}";
            }

            else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}/{item.Name}";
            }

            // TODO: throw error
            return "";
        }

        public string GetUrl(TypeScriptProperty item)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];
            string typeName = sourceType.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}/{item.Name}";
            }

            else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}/{item.Name}";
            }

            // TODO: throw error
            return "";
        }

        public string GetUrl(TypeScriptMethodSignature item)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];
            string typeName = sourceType.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}/{item.Name}";
            }

            else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}/{item.Name}";
            }

            // TODO: throw error
            return "";
        }

        public string GetUrl(IntrinsicType item)
        {
            return item.Name == "void" ? "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/void" : $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{item.Name}";
        }
    }
}
