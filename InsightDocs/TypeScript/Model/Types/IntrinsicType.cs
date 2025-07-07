using InsightDocs.Abstractions;
using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types
{
    public class IntrinsicType : TypeScriptType, ILinkTarget
    {
        [JsonProperty("name")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string Name
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            get;
            set;
        }

        public string LinkText
        {
            get
            {
                return Name;
            }
        }

        public override List<TypeToStringComponent> GetToStringComponents()
        {
            return
            [
                new TypeToStringIntrinsicTypeComponent(this)
            ];
        }
    }
}
