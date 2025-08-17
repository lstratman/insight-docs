type ReflectionHandler = (node: import('typescript').Node, typeChecker: import('typescript').TypeChecker) => TypeScriptType;

interface TypeScriptMetadata {
    modules?: { [moduleName: string]: TypeScriptModule };
    types?: { [typeName: string]: TypeScriptTypeDeclaration };
}

interface TypeScriptTypeLookup {
    symbol: import('typescript').Symbol,
    typeMetadata: TypeScriptTypeDeclaration;
}

type TypeDeclarationKind = 
    'interface'
    | 'alias'
    | 'enum'
    | 'variable';

interface TypeScriptTypeDeclaration {
    id: number;
    name: string;
    fullName: string;
    builtIn?: boolean;
    kind: TypeDeclarationKind;
    comment?: TypeScriptCommentMetadata;
}

interface TypeScriptEnum extends TypeScriptTypeDeclaration {
    kind: 'enum';
    values: { name: string }[];
}

interface TypeScriptVariable extends TypeScriptTypeDeclaration {
    kind: 'variable';
    type: TypeScriptType;
}

interface TypeScriptTypeAlias extends TypeScriptTypeDeclaration {
    kind: 'alias';
    type: TypeScriptType;
}

interface TypeScriptInterface extends TypeScriptTypeDeclaration {
    kind: 'interface';

    extends?: TypeScriptType[];
    typeParameters?: TypeScriptTypeParameter[];

    functionSignatures?: TypeScriptMethodSignature[];

    constructorMetadata?: TypeScriptMethod;
    indexSignatures?: TypeScriptMethodSignature[];
    methods?: { [methodName: string]: TypeScriptMethod };
    properties?: TypeScriptProperty[];
}

type CodeElementKind = 
    'method'
    | 'property'
    | 'signature';

interface CodeElement {
    kind: CodeElementKind;
    id: number;
    flags?: CodeElementFlags;
}

interface TypeScriptTypeParameter {
    name: string;
    constraint?: TypeScriptType;
}

interface TypeScriptProperty extends CodeElement {
    kind: 'property';
    name: string;
    comment?: TypeScriptCommentMetadata;
    type: TypeScriptType;
}

interface TypeScriptMethod extends CodeElement {
    kind: 'method';
    name: string;
    signatures: TypeScriptMethodSignature[];
}

interface TypeScriptMethodSignature extends CodeElement {
    kind: 'signature';
    name: string;
    comment?: TypeScriptCommentMetadata;
    returnType: TypeScriptType;
    parameters?: TypeScriptParameter[];
    typeParameters?: TypeScriptTypeParameter[];
}

interface TypeScriptParameter {
    kind: 'parameter';
    name: string;
    type: TypeScriptType;
    summary?: TypeScriptCommentTextSegment[];
    flags?: CodeElementFlags;
}

interface CodeElementFlags {
    isOptional?: boolean;
    isPrivate?: boolean;
    isProtected?: boolean;
}

type TypeScriptTypeKind = 
    'reference'
    | 'intrinsic'
    | 'this'
    | 'union'
    | 'intersection'
    | 'literal'
    | 'array'
    | 'function'
    | 'typeLiteral'
    | 'indexedAccess'
    | 'typeOperator'
    | 'typeParameter'
    | 'import'
    | 'tuple'
    | 'typePredicate'
    | 'typeQuery'
    | 'mappedType'
    | 'conditional'
    | 'inferred'
    | 'module';

interface TypeScriptType {
    kind: TypeScriptTypeKind;
}

interface TypeScriptInferredType extends TypeScriptType {
    kind: 'inferred';
    name: string;
}

interface TypeScriptConditionalType extends TypeScriptType {
    kind: 'conditional';
    checkType: TypeScriptType;
    extendsType: TypeScriptType;
    trueType: TypeScriptType;
    falseType: TypeScriptType;
}

interface TypeScriptLiteralType extends TypeScriptType {
    kind: 'literal';
    literalType: 'number' | 'string' | 'null' | 'boolean';
    literalValue: string;
}

interface TypeScriptThisType extends TypeScriptType {
    kind: 'this';
}

interface TypeScriptUnionType extends TypeScriptType {
    kind: 'union';
    types: TypeScriptType[];
}

interface TypeScriptIntersectionType extends TypeScriptType {
    kind: 'intersection';
    types: TypeScriptType[];
}

interface TypeScriptArrayType extends TypeScriptType {
    kind: 'array';
    elementType: TypeScriptType;
}

interface TypeScriptReferenceType extends TypeScriptType {
    kind: 'reference';
    id: number;
    typeArguments?: TypeScriptType[];
}

interface TypeScriptIntrinsicType extends TypeScriptType {
    kind: 'intrinsic';
    name: string;
}

interface TypeScriptFunctionType extends TypeScriptType {
    kind: 'function';
    returnType: TypeScriptType;
    parameters: TypeScriptParameter[];
}

interface TypeScriptTypeLiteralType extends TypeScriptType {
    kind: 'typeLiteral';
    members?: (TypeScriptProperty | TypeScriptMethod)[];
    indexSignature?: TypeScriptMethodSignature;
    constructorMetadata?: TypeScriptMethod;
}

interface TypeScriptIndexedAccessType extends TypeScriptType {
    kind: 'indexedAccess';
    indexType: TypeScriptType;
    objectType: TypeScriptType;
}

interface TypeScriptTypeOperatorType extends TypeScriptType {
    kind: 'typeOperator';
    operator: string;
    type: TypeScriptType;
}

interface TypeScriptTypeQueryType extends TypeScriptType {
    kind: 'typeQuery';
    type: TypeScriptType;
}

interface TypeScriptTypeParameterType extends TypeScriptType {
    kind: 'typeParameter';
    name: string;
}

interface TypeScriptImportType extends TypeScriptType {
    kind: 'import';
    argument: string;
}

interface TypeScriptTupleTypeElement {
    kind: 'tupleElement';
    name?: string;
    type: TypeScriptType;
}

interface TypeScriptTupleType extends TypeScriptType {
    kind: 'tuple';
    elements: TypeScriptTupleTypeElement[];
}

interface TypeScriptTypePredicate extends TypeScriptType {
    kind: 'typePredicate';
    parameterName: string;
    type: TypeScriptType;
}

interface TypeScriptMappedType extends TypeScriptType {
    kind: 'mappedType';
    typeParameter: TypeScriptTypeParameter;
    nameType?: TypeScriptType;
    type?: TypeScriptType;
}

interface TypeScriptModule extends TypeScriptType {
    kind: 'module';
    name: string;
    exportStyle: 'declaration' | 'instance' | 'list';
    exportType?: TypeScriptType;
    exportListItems?: (TypeScriptMethod | TypeScriptProperty)[];
}

interface TypeScriptCommentMetadata {
    summary?: TypeScriptCommentTextSegment[];
    returns?: TypeScriptCommentTextSegment[];
    blockTags?: TypeScriptCommentBlockTag[];
}

interface TypeScriptCommentBlockTag {
    tag: string;
    content?: TypeScriptCommentTextSegment[];
}

interface TypeScriptCommentTextSegment {
    kind: string;
    text: string;
}

interface TypeScriptCommentLinkSegment extends TypeScriptCommentTextSegment {
    kind: 'link';
}