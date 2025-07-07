using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types
{
    public class MappedType : TypeScriptType
    {
        [JsonProperty("typeParameter")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public TypeParameter Parameter
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            get;
            set;
        }

        [JsonProperty("type")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public TypeScriptType TemplateType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            get;
            set;
        }

        public override List<TypeToStringComponent> GetToStringComponents()
        {
            return
            [
                new TypeToStringTextComponent($"{{ [{Parameter.Name} in "),
                new TypeToStringTypeComponent(Parameter.Constraint!),
                new TypeToStringTextComponent($"]: "),
                new TypeToStringTypeComponent(TemplateType),
                new TypeToStringTextComponent(" }")
            ];
        }
    }
}
