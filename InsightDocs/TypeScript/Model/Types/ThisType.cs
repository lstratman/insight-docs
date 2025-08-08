
namespace InsightDocs.TypeScript.Model.Types;

public class ThisType : TypeScriptType
{
    public override List<TypeToStringComponent> GetToStringComponents()
    {
        return
        [
            new TypeToStringTextComponent("this")
        ];
    }
}
