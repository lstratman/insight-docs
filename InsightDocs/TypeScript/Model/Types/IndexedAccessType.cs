using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types
{
    public class IndexedAccessType : TypeScriptType
    {
        [JsonProperty("indexType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public TypeScriptType IndexType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            get;
            set;
        }

        [JsonProperty("objectType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public TypeScriptType ObjectType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            get;
            set;
        }

        public override string ToString()
        {
            return ObjectType.ToString() + "[" + IndexType.ToString() + "]";
        }
    }
}
