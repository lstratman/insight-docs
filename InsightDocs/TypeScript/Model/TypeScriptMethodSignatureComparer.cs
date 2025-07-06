namespace InsightDocs.TypeScript.Model;

public class TypeScriptMethodSignatureComparer : IEqualityComparer<TypeScriptMethodSignature>
{
    public bool Equals(TypeScriptMethodSignature? x, TypeScriptMethodSignature? y)
    {
        if (x != null && y != null)
        {
            return x.ToString() == y.ToString();
        }

        return x == null && y == null;
    }

    public int GetHashCode(TypeScriptMethodSignature obj)
    {
        return obj.ToString().GetHashCode();
    }
}
