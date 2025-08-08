using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace InsightDocs.TypeScript.Model.Types;

public class TypeScriptTypeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TypeScriptTypeConverter);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return ReadJson(JObject.Load(reader), serializer);
    }

    public static object ReadJson(JObject jsonObject, JsonSerializer serializer)
    {
        TypeScriptType typeObject;

        if (!jsonObject.ContainsKey("kind"))
        {
            throw new Exception("TypeScript type JSON object does not contain 'kind' property.");
        }

        switch (jsonObject["kind"]!.Value<string>())
        {
            case "union":
                typeObject = new UnionType();
                break;

            case "reference":
                typeObject = new ReferenceType();
                break;

            case "intrinsic":
                typeObject = new IntrinsicType();
                break;

            case "array":
                typeObject = new ArrayType();
                break;

            case "typeLiteral":
                typeObject = new ReflectionType();
                break;

            case "literal":
                if (!jsonObject.ContainsKey("literalType"))
                {
                    throw new Exception("Literal type JSON object does not contain 'literalType' property.");
                }

                typeObject = jsonObject["literalType"]!.ToString() switch
                {
                    "string" => new StringLiteralType(),
                    "number" => new NumericLiteralType(),
                    "boolean" => new BooleanLiteralType(),
                    "null" => new NullLiteralType(),
                    _ => throw new Exception("Unsupported literal type: " + jsonObject["literalType"]!.ToString() + "."),
                };

                break;

            case "tuple":
                typeObject = new TupleType();
                break;

            case "intersection":
                typeObject = new IntersectionType();
                break;

            case "typeQuery":
                typeObject = new QueryType();
                break;

            case "typeOperator":
                typeObject = new TypeOperatorType();
                break;

            case "typePredicate":
                typeObject = new PredicateType();
                break;

            case "indexedAccess":
                typeObject = new IndexedAccessType();
                break;

            case "mappedType":
                typeObject = new MappedType();
                break;

            case "typeParameter":
                typeObject = new TypeParameterType();
                break;

            case "function":
                typeObject = new FunctionType();
                break;

            case "this":
                typeObject = new ThisType();
                break;

            case "import":
                typeObject = new ImportType();
                break;

            case "conditional":
                typeObject = new ConditionalType();
                break;

            case "inferred":
                typeObject = new InferredType();
                break;

            default:
                throw new Exception("Unrecognized type for " + jsonObject["kind"]!.Value<string>() + ".");
        }

        serializer.Populate(jsonObject.CreateReader(), typeObject);
        return typeObject;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public abstract class TypeToStringComponent
{
    public abstract override string ToString();
}

public class TypeToStringReferenceTypeComponent(int id) : TypeToStringComponent
{
    public int Id
    {
        get;
        set;
    } = id;

    public override string ToString()
    {
        string typeName = ReferenceType.AllTypes[Id].Name;

        if (typeName.Contains('<'))
        {
            typeName = typeName[..typeName.IndexOf('<')];
        }

        return typeName;
    }
}

public class TypeToStringIntrinsicTypeComponent(IntrinsicType type) : TypeToStringComponent
{
    public IntrinsicType Type
    {
        get;
        set;
    } = type;

    public override string ToString()
    {
        return Type.Name;
    }
}

public class TypeToStringTypeComponent(TypeScriptType type) : TypeToStringComponent
{
    public TypeScriptType Type
    {
        get;
        set;
    } = type;

    public override string ToString()
    {
        return Type.ToString();
    }
}

public class TypeToStringTextComponent(string text) : TypeToStringComponent
{
    public string Text
    {
        get;
        set;
    } = text;

    public override string ToString()
    {
        return Text;
    }
}

[JsonConverter(typeof(TypeScriptTypeConverter))]
public abstract class TypeScriptType
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public virtual string Kind
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public abstract List<TypeToStringComponent> GetToStringComponents();

    public override string ToString()
    {
        StringBuilder output = new StringBuilder();

        foreach (TypeToStringComponent component in GetToStringComponents())
        {
            output.Append(component.ToString());
        }

        return output.ToString();
    }

    public static TypeScriptType GetTypeScriptType(string type)
    {
        if (type.Contains('<'))
        {
            ReferenceType referenceType = new ReferenceType
            {
                Arguments = []
            };

            string name = type[..type.IndexOf('<')];
            referenceType.Target = ReferenceType.AllTypesByName.ContainsKey(name) ? ReferenceType.AllTypesByName[name].Id : -1;

            if (referenceType.Target == -1)
            {
                KeyValuePair<string, TypeScriptTypeDeclaration> genericType = ReferenceType.AllTypesByName.FirstOrDefault(k => k.Key.StartsWith(name + "<"));

                if (genericType.Value != null)
                {
                    referenceType.Target = genericType.Value.Id;
                }
            }

            string genericArguments = type[type.IndexOf('<')..];
            genericArguments = genericArguments[1..^1];

            int bracketStack = 0;
            StringBuilder currentType = new StringBuilder();

            for (int i = 0; i < genericArguments.Length; i++)
            {
                if (genericArguments[i] == '<')
                {
                    currentType.Append(genericArguments[i]);
                    bracketStack++;
                }

                else if (genericArguments[i] == '>')
                {
                    currentType.Append(genericArguments[i]);
                    bracketStack--;
                }

                else if (genericArguments[i] == ',')
                {
                    if (bracketStack == 0)
                    {
                        referenceType.Arguments.Add(GetTypeScriptType(currentType.ToString().Trim()));
                        currentType.Length = 0;
                    }

                    else
                    {
                        currentType.Append(genericArguments[i]);
                    }
                }

                else
                {
                    currentType.Append(genericArguments[i]);
                }
            }

            referenceType.Arguments.Add(GetTypeScriptType(currentType.ToString().Trim()));

            return referenceType;
        }

        else if (type.Contains('.'))
        {
            int target = ReferenceType.AllTypesByName.TryGetValue(type, out TypeScriptTypeDeclaration? value) ? value.Id : -1;

            if (target == -1)
            {
                KeyValuePair<string, TypeScriptTypeDeclaration> genericType = ReferenceType.AllTypesByName.FirstOrDefault(k => k.Key.StartsWith(type + "<"));

                if (genericType.Value != null)
                {
                    target = genericType.Value.Id;
                }
            }

            return new ReferenceType
            {
                Target = target
            };
        }

        else
        {
            return new IntrinsicType
            {
                Name = type
            };
        }
    }
}
