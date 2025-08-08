namespace InsightDocs.TypeScript.Model;

public class TypeScriptMethodSignatureComparer : IEqualityComparer<TypeScriptMethodSignature>
{
    public bool Equals(TypeScriptMethodSignature? x, TypeScriptMethodSignature? y)
    {
        return x != null && y != null ? x.ToString() == y.ToString() : x == null && y == null;
    }

    public int GetHashCode(TypeScriptMethodSignature obj)
    {
        return obj.ToString().GetHashCode();
    }
}
