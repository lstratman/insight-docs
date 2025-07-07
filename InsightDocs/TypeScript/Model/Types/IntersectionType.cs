using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types
{
    public class IntersectionType: TypeScriptType
    {
        [JsonProperty("types")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public List<TypeScriptType> Types
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            get;
            set;
        }

        public override List<TypeToStringComponent> GetToStringComponents()
        {
            List<TypeToStringComponent> components = [];

            foreach (TypeScriptType type in Types)
            {
                if (components.Count > 0)
                {
                    components.Add(new TypeToStringTextComponent(" & "));
                }

                components.Add(new TypeToStringTypeComponent(type));
            }

            return components;
        }
    }
}
