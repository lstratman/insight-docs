using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class TupleType : TypeScriptType
{
    [JsonProperty("elements")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public List<TupleTypeElement> Elements
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override List<TypeToStringComponent> GetToStringComponents()
    {
        List<TypeToStringComponent> components = [new TypeToStringTextComponent("[")];

        bool first = true;

        foreach (TupleTypeElement element in Elements)
        {
            if (!first)
            {
                components.Add(new TypeToStringTextComponent(", "));
            }

            first = false;

            if (!String.IsNullOrEmpty(element.Name))
            {
                components.Add(new TypeToStringTextComponent(element.Name + ": "));
            }

            components.Add(new TypeToStringTypeComponent(element.Type));
        }

        components.Add(new TypeToStringTextComponent("]"));
        return components;
    }
}

public class TupleTypeElement
{
    [JsonProperty("name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonProperty("type")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType Type
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }
}
