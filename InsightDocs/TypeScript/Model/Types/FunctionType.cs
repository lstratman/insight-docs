using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class FunctionType : TypeScriptType
{
    [JsonProperty("name")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Name
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("returnType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType ReturnType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("parameters")]
    public List<TypeScriptParameter>? Parameters
    {
        get;
        set;
    }

    public override List<TypeToStringComponent> GetToStringComponents()
    {
        List<TypeToStringComponent> components = [];
        components.Add(new TypeToStringTextComponent("("));

        if (Parameters != null && Parameters.Count > 0)
        {
            
            foreach (TypeScriptParameter parameter in Parameters)
            {
                if (components.Count > 1)
                {
                    components.Add(new TypeToStringTextComponent(", "));
                }

                components.Add(new TypeToStringTypeComponent(parameter.Type));
                components.Add(new TypeToStringTextComponent(" " + parameter.Name));
            }
        }

        components.Add(new TypeToStringTextComponent(") => "));
        components.Add(new TypeToStringTypeComponent(ReturnType));

        return components;
    }
}
