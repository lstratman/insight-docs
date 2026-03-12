using InsightDocs.TypeScript.Model.Types;
using InsightDocs.TypeScript.Services;
using Newtonsoft.Json;
using System.Text;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptInterface : TypeScriptNamespacedTypeDeclaration
{
    protected bool _normalized = false;

    public static readonly string[] HTMLElementTypes =
    [
        "ARIAMixin",
        "Slottable",
        "GlobalEventHandlers",
        "EventTarget",
        "HTMLOrSVGElement",
        "ElementContentEditable",
        "NonDocumentTypeChildNode",
        "Animatable"
    ];

    public static readonly string[] NodeTypes =
    [
        "ParentNode",
        "ChildNode"
    ];

    public static readonly string[] BuiltInTypes =
    [
        "_File",
        "_RequestInit",
        "_Request",
        "_Response",
        "_Headers",
        "_FormData",
        "__EventTarget",
        "__Event",
        "buffer.__Blob",
        "url._URL",
        "url._URLSearchParams",
        "Pick",
        "Window"
    ];

    public override string Name
    {
        get
        {
            string displayName = base.Name;

            if (TypeParameters != null && TypeParameters.Count > 0)
            {
                displayName += "<";

                for (int i = 0; i < TypeParameters.Count; i++)
                {
                    if (i > 0)
                    {
                        displayName += ", ";
                    }

                    displayName += TypeParameters[i].Name;
                }

                displayName += ">";
            }

            return displayName;
        }

        set
        {
            base.Name = value;
        }
    }

    [JsonProperty("properties")]
    public List<TypeScriptProperty>? Properties
    {
        get;
        set;
    }

    [JsonProperty("methods")]
    public Dictionary<string, TypeScriptMethod>? Methods
    {
        get;
        set;
    }

    [JsonProperty("constructorMetadata")]
    public TypeScriptMethod? Constructor
    {
        get;
        set;
    }

    [JsonProperty("typeParameters")]
    public List<TypeParameter>? TypeParameters
    {
        get;
        set;
    }

    [JsonProperty("extends")]
    public List<TypeScriptType>? Extends
    {
        get;
        set;
    }

    [JsonProperty("indexSignatures")]
    public List<TypeScriptMethodSignature>? IndexSignatures
    {
        get;
        set;
    }

    [JsonProperty("functionSignatures")]
    public TypeScriptMethod? FunctionSignatures
    {
        get;
        set;
    }

    public virtual TypeScriptModule? ExportedFromModule
    {
        get;
        set;
    }

    public override string ToString()
    {
        if (ExportedFromModule != null)
        {
            return $"import {Name} from '{ExportedFromModule.FullName}'";
        }

        else
        {
            StringBuilder output = new StringBuilder("interface " + Name);

            if (Extends != null && Extends.Count > 0)
            {
                output.Append(" extends ");

                for (int i = 0; i < Extends.Count; i++)
                {
                    if (i > 0)
                    {
                        output.Append(", ");
                    }

                    output.Append(ReferenceType.AllTypes[((ReferenceType)Extends[i]).Target].Name);
                }
            }

            return output.ToString();
        }
    }

    public override string Title
    {
        get
        {
            return ExportedFromModule != null ? ExportedFromModule.Title : Name + (FunctionSignatures != null ? " Function" : " Interface");
        }
    }

    protected static string? GetMDNUrl(TypeScriptCodeElement member)
    {
        if (member.Comment != null && member.Comment.SummaryMarkdown != null && member.Comment.SummaryMarkdown.Any(m => m.Text.Contains("[MDN Reference](")))
        {
            CommentSegment mdnLinkSegment = member.Comment.SummaryMarkdown.First(m => m.Text.Contains("[MDN Reference]("));
            int startIndex = mdnLinkSegment.Text.IndexOf("[MDN Reference](") + 16;
            int endIndex = mdnLinkSegment.Text.IndexOf(')', startIndex);
            string mdnUrl = mdnLinkSegment.Text[startIndex..endIndex];

            if (MDNUrlResolver.MDNUrlMappings.TryGetValue(mdnUrl, out string? value))
            {
                mdnUrl = value;
                mdnLinkSegment.Text = "[MDN Reference](" + mdnUrl + ")";
            }

            return mdnUrl;
        }

        return null;
    }

    public override void Normalize()
    {
        if (_normalized)
        {
            return;
        }

        int id = Id;
        string name = Name;

        if (BuiltInTypes.Contains(FullName))
        {
            BuiltIn = true;
        }

        if (HTMLElementTypes.Contains(name))
        {
            id = ReferenceType.AllTypes.Values.First(a => a.Name == "HTMLElement").Id;
        }

        else if (NodeTypes.Contains(name))
        {
            id = ReferenceType.AllTypes.Values.First(a => a.Name == "Node").Id;
        }

        if (Properties != null)
        {
            foreach (TypeScriptProperty property in Properties)
            {
                if (property.Name.StartsWith('@'))
                {
                    property.Name = property.Name[1..];
                }

                property.SourceTypeId = id;
            }
        }

        TypeScriptMethodSignatureComparer methodSignatureComparer = new TypeScriptMethodSignatureComparer();

        if (Methods != null)
        {
            List<string> removeMethods = [];

            foreach (string methodName in Methods.Keys.Where(k => k.StartsWith('@')))
            {
                string newMethodName = methodName[1..];

                foreach (TypeScriptMethodSignature signature in Methods[methodName].Signatures)
                {
                    signature.Name = newMethodName;
                }

                Methods[methodName].Name = newMethodName;
                removeMethods.Add(methodName);
            }

            if (removeMethods.Count != 0)
            {
                foreach (string methodName in removeMethods)
                {
                    Methods[methodName[1..]] = Methods[methodName];
                    Methods.Remove(methodName);
                }
            }

            foreach (KeyValuePair<string, TypeScriptMethod> method in Methods)
            {
                method.Value.SourceTypeId = id;

                if (method.Value.Signatures.Count > 1)
                {
                    method.Value.Signatures = [.. method.Value.Signatures.Distinct(methodSignatureComparer)];
                }

                foreach (TypeScriptMethodSignature signature in method.Value.Signatures)
                {
                    signature.SourceTypeId = id;
                    signature.MethodCollection = method.Value;
                }
            }
        }

        if (Constructor != null)
        {
            Constructor.SourceTypeId = id;

            foreach (TypeScriptMethodSignature signature in Constructor.Signatures)
            {
                signature.SourceTypeId = id;
            }
        }

        if (IndexSignatures != null)
        {
            foreach (TypeScriptMethodSignature indexSignature in IndexSignatures)
            {
                indexSignature.SourceTypeId = id;
            }
        }

        if (FunctionSignatures != null)
        {
            FunctionSignatures.SourceTypeId = id;
            FunctionSignatures.Signatures = [.. FunctionSignatures.Signatures.Distinct(methodSignatureComparer)];

            foreach (TypeScriptMethodSignature functionSignature in FunctionSignatures.Signatures)
            {
                functionSignature.SourceTypeId = id;
                functionSignature.MethodCollection = FunctionSignatures;
            }
        }

        if (Extends != null)
        {
            for (int i = Extends.Count - 1; i >= 0; i--)
            {
                TypeScriptType baseType = Extends[i];

                if (baseType is ReferenceType baseTypeReference)
                {
                    if (!ReferenceType.AllTypes.ContainsKey(baseTypeReference.Target))
                    {
                        throw new Exception($"In TypeScriptInterface for \"{FullName}\", the AllTypes Dictionary doesn't contain an extends target ID ({baseTypeReference.Target}).");
                    }

                    TypeScriptInterface? actualBaseType = ReferenceType.AllTypes[baseTypeReference.Target] as TypeScriptInterface;

                    if (actualBaseType == null && ReferenceType.AllTypes[baseTypeReference.Target] is TypeScriptTypeAlias typeAliasBaseType)
                    {
                        if (typeAliasBaseType.Type is ReferenceType referenceType)
                        {
                            actualBaseType = ReferenceType.AllTypes[referenceType.Target] as TypeScriptInterface;
                        }
                    }

                    if (actualBaseType == null)
                    {
                        continue;
                    }

                    actualBaseType.Normalize();

                    if (actualBaseType.Methods != null)
                    {
                        foreach (KeyValuePair<string, TypeScriptMethod> method in actualBaseType.Methods)
                        {
                            Methods ??= [];

                            if (!Methods.ContainsKey(method.Key))
                            {
                                Methods[method.Key] = method.Value;
                            }

                            else
                            {
                                foreach (TypeScriptMethodSignature signature in method.Value.Signatures)
                                {
                                    if (!Methods[method.Key].Signatures.Contains(signature, methodSignatureComparer))
                                    {
                                        Methods[method.Key].Signatures.Add(signature);
                                    }
                                }
                            }
                        }
                    }

                    if (actualBaseType.Properties != null)
                    {
                        foreach (TypeScriptProperty property in actualBaseType.Properties)
                        {
                            Properties ??= [];

                            if (!Properties.Any(p => p.Name == property.Name))
                            {
                                Properties.Add(property);
                            }
                        }
                    }

                    if (actualBaseType.IndexSignatures != null)
                    {
                        foreach (TypeScriptMethodSignature indexSignature in actualBaseType.IndexSignatures)
                        {
                            IndexSignatures ??= [];

                            if (!IndexSignatures.Contains(indexSignature, methodSignatureComparer))
                            {
                                IndexSignatures.Add(indexSignature);
                            }
                        }
                    }

                    if (Constructor == null && actualBaseType.Constructor != null)
                    {
                        Constructor = actualBaseType.Constructor;
                    }
                }
            }
        }

        if (Methods != null)
        {
            foreach (TypeScriptMethod method in Methods.Values)
            {
                method.Signatures = [.. method.Signatures.Distinct(methodSignatureComparer)];
            }
        }

        _normalized = true;
    }
}
