
namespace InsightDocs.TypeScript.Model.Types;

public class NullLiteralType : TypeScriptType
{
    public override List<TypeToStringComponent> GetToStringComponents()
    {
        return
        [
            new TypeToStringTextComponent("null")
        ];
    }
}
