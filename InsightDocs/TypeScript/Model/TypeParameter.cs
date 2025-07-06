using InsightDocs.TypeScript.Model.Types;
using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model;

public class TypeParameter
{
    public class Comparer : IEqualityComparer<TypeParameter>
    {
        public bool Equals(TypeParameter? x, TypeParameter? y)
        {
            if (x != null && y != null)
            {
                return x.Name == y.Name;
            }

            return x == null && y == null;
        }

        public int GetHashCode(TypeParameter obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    [JsonProperty("name")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Name
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("constraint")]
    public TypeScriptType? Constraint
    {
        get;
        set;
    }
}
