using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.TypeScript.Model.Types;

public class ReflectionType : TypeScriptType
{
    [JsonProperty("members")]
    public List<TypeScriptCodeElement>? Members
    {
        get;
        set;
    }

    [JsonProperty("indexSignature")]
    public TypeScriptMethodSignature? IndexSignature
    {
        get;
        set;
    }

    public override List<TypeToStringComponent> GetToStringComponents()
    {
        List<TypeToStringComponent> components = [new TypeToStringTextComponent("{ ")];

        if (IndexSignature != null && IndexSignature.Parameters != null)
        {
            components.Add(new TypeToStringTextComponent("["));
            
            bool first = true;

            foreach (TypeScriptParameter param in IndexSignature.Parameters)
            {
                if (!first)
                {
                    components.Add(new TypeToStringTextComponent(", "));
                }

                first = false;

                components.Add(new TypeToStringTextComponent(param.Name + ": "));
                components.Add(new TypeToStringTypeComponent(param.Type));
            }

            components.Add(new TypeToStringTextComponent("]: "));
            components.Add(new TypeToStringTypeComponent(IndexSignature.Type));
        }

        if (Members != null)
        {
            if (IndexSignature != null && IndexSignature.Parameters != null)
            {
                components.Add(new TypeToStringTextComponent(", "));
            }

            bool first = true;

            foreach (TypeScriptCodeElement member in Members)
            {
                if (!first)
                {
                    components.Add(new TypeToStringTextComponent(", "));
                }

                first = false;

                if (member is TypeScriptProperty property)
                {
                    components.Add(new TypeToStringTextComponent(property.Name + ": "));
                    components.Add(new TypeToStringTypeComponent(property.Type));
                }

                else if (member is TypeScriptMethod method)
                {
                    components.Add(new TypeToStringTextComponent(method.Name + ": ("));

                    if (method.Signatures[0].Parameters != null)
                    {
                        bool firstParam = true;

                        foreach (TypeScriptParameter param in method.Signatures[0].Parameters!)
                        {
                            if (!firstParam)
                            {
                                components.Add(new TypeToStringTextComponent(", "));
                            }

                            firstParam = false;

                            components.Add(new TypeToStringTextComponent(param.Name + ": "));
                            components.Add(new TypeToStringTypeComponent(param.Type));
                        }
                    }

                    components.Add(new TypeToStringTextComponent(") => "));
                    components.Add(new TypeToStringTypeComponent(method.Signatures[0].ReturnType));
                }
            }
        }

        components.Add(new TypeToStringTextComponent(" }"));

        return components;
    }
}
