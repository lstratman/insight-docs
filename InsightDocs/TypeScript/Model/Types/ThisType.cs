
namespace InsightDocs.TypeScript.Model.Types;

public class ThisType : TypeScriptType
{
    public override List<TypeToStringComponent> GetToStringComponents()
    {
        return new List<TypeToStringComponent>
        {
            new TypeToStringTextComponent("this")
        };
    }
}
