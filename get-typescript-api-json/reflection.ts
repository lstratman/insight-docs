import * as ts from 'typescript';
import NodeError from './node-error.js';
import * as path from 'path';

let nodeHandlers: { [kind: number]: ReflectionHandler } = {};
let typeIdCounter: number = 1;
let codeElementIdCounter: number = 1;

export let typesLookup: Map<number, TypeScriptTypeLookup> = new Map();
export let typesLookupByName: Map<string, TypeScriptTypeLookup> = new Map();

export function getPropertyName(nameNode: ts.PropertyName | ts.MemberName | ts.BindingName): string {
    if (nameNode.kind === ts.SyntaxKind.Identifier) {
        return (<ts.Identifier>nameNode).text;
    }

    else if (nameNode.kind === ts.SyntaxKind.StringLiteral) {
        return (<ts.StringLiteral>nameNode).text;
    }

    else if (nameNode.kind === ts.SyntaxKind.NoSubstitutionTemplateLiteral) {
        return (<ts.NoSubstitutionTemplateLiteral>nameNode).text;
    }

    else if (nameNode.kind === ts.SyntaxKind.NumericLiteral) {
        return (<ts.NumericLiteral>nameNode).text;
    }

    else if (nameNode.kind === ts.SyntaxKind.ComputedPropertyName) {
        return (<ts.ComputedPropertyName>nameNode).expression.getText();
    }

    else if (nameNode.kind === ts.SyntaxKind.PrivateIdentifier) {
        return (<ts.PrivateIdentifier>nameNode).text;
    }

    else if (nameNode.kind === ts.SyntaxKind.ArrayBindingPattern) {
        return (<ts.ArrayBindingPattern>nameNode).elements[0].getText();
    }

    else {
        throw new NodeError(`Encountered unsupported property name kind: ${nameNode.kind}`, nameNode);
    }
}

export function registerTypeNodeForReflection(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    if (!node) {
        return <TypeScriptIntrinsicType> {
            kind: 'intrinsic',
            name: 'any'
        };
    }

    if (nodeHandlers[node.kind]) {
        return nodeHandlers[node.kind](node, typeChecker);
    }

    else {
        throw new NodeError(`Encountered unsupported node kind: ${node.kind}`, node);
    }
}

export function registerSymbolForReflection(symbol: ts.Symbol, typeChecker: ts.TypeChecker): TypeScriptTypeDeclaration {
    if (!symbol.typeId) {
        if (typesLookupByName.has(symbol.name)) {
            symbol.typeId = typesLookupByName.get(symbol.name).symbol.typeId;
            return typesLookupByName.get(symbol.name).typeMetadata;
        }

        symbol.typeId = typeIdCounter++;
    }

    if (typesLookup.has(symbol.typeId)) {
        return typesLookup.get(symbol.typeId).typeMetadata;
    }

    let typeLookup: TypeScriptTypeLookup = {
        symbol: symbol,
        typeMetadata: {
            kind: 'interface',
            id: symbol.typeId,
            name: symbol.name,
            fullName: symbol.name
        }
    };

    let modulePath: string;
    let currentDeclarationContainer: ts.Node = symbol.declarations[0];

    while (currentDeclarationContainer) {
        if (currentDeclarationContainer.kind === ts.SyntaxKind.ModuleDeclaration) {
            modulePath = (<ts.ModuleDeclaration>currentDeclarationContainer).name.text;
            typeLookup.typeMetadata.fullName = (<ts.ModuleDeclaration>currentDeclarationContainer).name.text + '.' + typeLookup.typeMetadata.fullName;
        }

        else if (currentDeclarationContainer.kind === ts.SyntaxKind.SourceFile) {
            let sourceFile = currentDeclarationContainer as ts.SourceFile;

            if (sourceFile.fileName.indexOf('/lib.') !== -1) {
                typeLookup.typeMetadata.builtIn = true;
            }
        }

        currentDeclarationContainer = currentDeclarationContainer.parent;
    }

    typesLookup.set(symbol.typeId, typeLookup);

    let extendsList: TypeScriptType[] = null;

    for (let declaration of symbol.declarations) {
        if (declaration.kind === ts.SyntaxKind.InterfaceDeclaration || declaration.kind === ts.SyntaxKind.ClassDeclaration) {
            let interfaceDeclaration = declaration as (ts.InterfaceDeclaration | ts.ClassDeclaration);
            let interfaceSymbol = interfaceDeclaration.symbol || typeChecker.getSymbolAtLocation(interfaceDeclaration);
            let interfaceComments = getCommentMetadata(interfaceSymbol, typeChecker);
            let typeMetadata = typeLookup.typeMetadata as TypeScriptInterface;

            typeMetadata.kind = 'interface';

            if (interfaceComments) {
                typeLookup.typeMetadata.comment = interfaceComments;
            }

            if (interfaceDeclaration.heritageClauses) {
                for (let heritageClause of interfaceDeclaration.heritageClauses) {
                    for (let heritageType of heritageClause.types) {
                        if (!extendsList) {
                            extendsList = [];
                        }

                        extendsList.push(registerTypeNodeForReflection(heritageType.expression, typeChecker));
                    }
                }
            }

            if (interfaceDeclaration.typeParameters) {
                for (let typeParameterNode of interfaceDeclaration.typeParameters) {
                    let typeParameter: TypeScriptTypeParameter = {
                        name: typeParameterNode.name.text
                    };
    
                    if (typeParameterNode.constraint) {
                        typeParameter.constraint = registerTypeNodeForReflection(typeParameterNode.constraint, typeChecker);
                    }
    
                    if (!typeMetadata.typeParameters) {
                        typeMetadata.typeParameters = [];
                    }
    
                    if (!typeMetadata.typeParameters.find(p => p.name === typeParameter.name)) {
                        typeMetadata.typeParameters.push(typeParameter);
                    }
                }
            }

            if (interfaceDeclaration.members) {
                let emptyObject = {};

                for (let memberDeclaration of interfaceDeclaration.members) {
                    let memberSymbol = memberDeclaration.symbol || typeChecker.getSymbolAtLocation(memberDeclaration);
                    let memberComments = getCommentMetadataForMember(memberDeclaration, typeChecker);
                    let memberJsDocTags = getJsDocTags(memberSymbol, typeChecker);

                    if (memberDeclaration.kind === ts.SyntaxKind.PropertySignature || memberDeclaration.kind === ts.SyntaxKind.PropertyDeclaration) {
                        let propertySignature = memberDeclaration as (ts.PropertySignature | ts.PropertyDeclaration);
        
                        if (!typeMetadata.properties) {
                            typeMetadata.properties = [];
                        }
        
                        let property: TypeScriptProperty = {
                            id: codeElementIdCounter++,
                            kind: 'property',
                            name: getPropertyName(memberDeclaration.name),
                            type: registerTypeNodeForReflection(propertySignature.type, typeChecker)
                        };

                        if (typeMetadata.properties.find(p => p.name === property.name)) {
                            continue;
                        }

                        if (memberComments) {
                            property.comment = memberComments;
                        }

                        typeMetadata.properties.push(property);
                    }
        
                    else if (memberDeclaration.kind === ts.SyntaxKind.MethodSignature || memberDeclaration.kind === ts.SyntaxKind.MethodDeclaration) {
                        let methodSignature = memberDeclaration as (ts.MethodSignature | ts.MethodDeclaration);
                        let methodName = getPropertyName(memberDeclaration.name);

                        if (emptyObject[methodName]) {
                            methodName = '@' + methodName;
                        }
        
                        if (!typeMetadata.methods) {
                            typeMetadata.methods = {};
                        }
        
                        if (!typeMetadata.methods[methodName]) {
                            typeMetadata.methods[methodName] = {
                                id: codeElementIdCounter++,
                                kind: 'method',
                                name: methodName,
                                signatures: []
                            };
                        }
        
                        let methodSignatureMetadata = getMethodSignatureMetadata(methodSignature, methodName, typeChecker, memberJsDocTags);

                        if (memberComments) {
                            methodSignatureMetadata.comment = memberComments;

                            if (methodSignatureMetadata.parameters && memberComments.blockTags) {
                                for (let parameter of methodSignatureMetadata.parameters) {
                                    let parameterBlockTag = memberComments.blockTags.find(t => t.tag === '@param' && t.content.length > 1 && t.content[0].text === parameter.name);

                                    if (parameterBlockTag) {
                                        parameter.summary = [...parameterBlockTag.content];
                                        parameter.summary.splice(0, 1);
                                    }
                                }
                            }
                        }

                        typeMetadata.methods[methodName].signatures.push(methodSignatureMetadata);
                    }
        
                    else if (memberDeclaration.kind === ts.SyntaxKind.ConstructSignature || memberDeclaration.kind === ts.SyntaxKind.Constructor) {
                        let methodSignature = memberDeclaration as (ts.ConstructSignatureDeclaration | ts.ConstructorDeclaration);
                                
                        if (!typeMetadata.constructorMetadata) {
                            typeMetadata.constructorMetadata = {
                                id: codeElementIdCounter++,
                                kind: 'method',
                                name: 'constructor',
                                signatures: []
                            };
                        }

                        let methodSignatureMetadata = getMethodSignatureMetadata(methodSignature, 'constructor', typeChecker, memberJsDocTags);

                        if (memberComments) {
                            methodSignatureMetadata.comment = memberComments;

                            if (methodSignatureMetadata.parameters && memberComments.blockTags) {
                                for (let parameter of methodSignatureMetadata.parameters) {
                                    let parameterBlockTag = memberComments.blockTags.find(t => t.tag === '@param' && t.content.length > 1 && t.content[0].text === parameter.name);

                                    if (parameterBlockTag) {
                                        parameter.summary = [...parameterBlockTag.content];
                                        parameter.summary.splice(0, 1);
                                    }
                                }
                            }
                        }
        
                        typeMetadata.constructorMetadata.signatures.push(methodSignatureMetadata);
                    }
        
                    else if (memberDeclaration.kind === ts.SyntaxKind.IndexSignature) {
                        let methodSignature = memberDeclaration as ts.IndexSignatureDeclaration;
                                
                        if (!typeMetadata.indexSignatures) {
                            typeMetadata.indexSignatures = [];
                        }

                        let methodSignatureMetadata = getMethodSignatureMetadata(methodSignature, '', typeChecker, memberJsDocTags);

                        if (memberComments) {
                            methodSignatureMetadata.comment = memberComments;

                            if (methodSignatureMetadata.parameters && memberComments.blockTags) {
                                for (let parameter of methodSignatureMetadata.parameters) {
                                    let parameterBlockTag = memberComments.blockTags.find(t => t.tag === '@param' && t.content.length > 1 && t.content[0].text === parameter.name);

                                    if (parameterBlockTag) {
                                        parameter.summary = [...parameterBlockTag.content];
                                        parameter.summary.splice(0, 1);
                                    }
                                }
                            }
                        }
        
                        typeMetadata.indexSignatures.push(methodSignatureMetadata);
                    }
        
                    else if (memberDeclaration.kind === ts.SyntaxKind.CallSignature) {
                        let callSignature = memberDeclaration as ts.CallSignatureDeclaration;
                        let callSignatureMetadata = getMethodSignatureMetadata(callSignature, '', typeChecker, memberJsDocTags);

                        if (memberComments) {
                            callSignatureMetadata.comment = memberComments;

                            if (callSignatureMetadata.parameters && memberComments.blockTags) {
                                for (let parameter of callSignatureMetadata.parameters) {
                                    let parameterBlockTag = memberComments.blockTags.find(t => t.tag === '@param' && t.content.length > 1 && t.content[0].text === parameter.name);

                                    if (parameterBlockTag) {
                                        parameter.summary = [...parameterBlockTag.content];
                                        parameter.summary.splice(0, 1);
                                    }
                                }
                            }
                        }

                        callSignatureMetadata.name = typeMetadata.name.substr(0, 1).toLowerCase() + typeMetadata.name.substr(1);

                        if (!typeMetadata.functionSignatures) {
                            typeMetadata.functionSignatures = {
                                kind: 'method',
                                name: callSignatureMetadata.name,
                                signatures: [],
                                id: codeElementIdCounter++,
                                isFunctionSignatures: true
                            };
                        }
        
                        typeMetadata.functionSignatures.signatures.push(callSignatureMetadata);
                    }
        
                    else if (memberDeclaration.kind === ts.SyntaxKind.GetAccessor) {
                        // TODO
                    }
        
                    else if (memberDeclaration.kind === ts.SyntaxKind.SetAccessor) {
                        // TODO
                    }
        
                    else {
                        throw new NodeError(`Encountered unsupported member type kind: ${memberDeclaration.kind}`, memberDeclaration);
                    }
                }
            }

            if (!symbol.declarations.every(d => d.kind === ts.SyntaxKind.InterfaceDeclaration)) {
                break;
            }
        }

        else if (declaration.kind === ts.SyntaxKind.FunctionDeclaration) {
            let functionDeclaration = declaration as ts.FunctionDeclaration;
            let functionJsDocTags = getJsDocTags(symbol, typeChecker);
            let functionComments = getCommentMetadata(symbol, typeChecker);
            let functionSignatureMetadata = getMethodSignatureMetadata(functionDeclaration, '', typeChecker, functionJsDocTags);
            let typeMetadata = typeLookup.typeMetadata as TypeScriptInterface;

            typeMetadata.kind = 'interface';

            if (functionComments) {
                functionSignatureMetadata.comment = functionComments;

                if (functionSignatureMetadata.parameters && functionComments.blockTags) {
                    for (let parameter of functionSignatureMetadata.parameters) {
                        let parameterBlockTag = functionComments.blockTags.find(t => t.tag === '@param' && t.content.length > 1 && t.content[0].text === parameter.name);

                        if (parameterBlockTag) {
                            parameter.summary = [...parameterBlockTag.content];
                            parameter.summary.splice(0, 1);
                        }
                    }
                }
            }

            let fileName = path.basename(modulePath);

            if (fileName.endsWith('.js')) {
                fileName = fileName.substr(0, fileName.length - 3);
            }

            typeLookup.typeMetadata.name = fileName.substr(0, 1).toUpperCase() + fileName.substr(1).replace(/-[a-zA-Z]/g, (substring) => substring.substr(1, 1).toUpperCase());
            typeLookup.typeMetadata.fullName = 'quippe.' + typeLookup.typeMetadata.name;

            functionSignatureMetadata.name = typeLookup.typeMetadata.name.substr(0, 1).toLowerCase() + typeLookup.typeMetadata.name.substr(1);

            if (!typeMetadata.functionSignatures) {
                typeMetadata.functionSignatures = {
                    kind: 'method',
                    id: codeElementIdCounter++,
                    name: functionSignatureMetadata.name,
                    isFunctionSignatures: true,
                    signatures: []
                }
            }

            typeMetadata.functionSignatures.signatures.push(functionSignatureMetadata);

            break;
        }

        else if (declaration.kind === ts.SyntaxKind.TypeAliasDeclaration) {
            let typeMetadata = typeLookup.typeMetadata as TypeScriptTypeAlias;
            let aliasDeclaration = declaration as ts.TypeAliasDeclaration;
            let aliasComments = getCommentMetadata(symbol, typeChecker);

            typeMetadata.kind = 'alias';

            if (aliasComments) {
                typeMetadata.comment = aliasComments;
            }

            typeMetadata.type = registerTypeNodeForReflection(aliasDeclaration.type, typeChecker);
        }

        else if (declaration.kind === ts.SyntaxKind.VariableDeclaration) {
            let typeMetadata = typeLookup.typeMetadata as TypeScriptVariable;
            let variableDeclaration = declaration as ts.VariableDeclaration;

            typeMetadata.kind = 'variable';
            typeMetadata.type = registerTypeNodeForReflection(variableDeclaration.type, typeChecker);
        }

        else if (declaration.kind === ts.SyntaxKind.ImportSpecifier) {
            let typeMetadata = typeLookup.typeMetadata as TypeScriptTypeAlias;
            let importSpecifierDeclaration = declaration as ts.ImportSpecifier;

            typeMetadata.kind = 'alias';
            typeMetadata.type = registerTypeNodeForReflection(importSpecifierDeclaration.propertyName, typeChecker);
        }

        else if (declaration.kind === ts.SyntaxKind.EnumDeclaration) {
            let typeMetadata = typeLookup.typeMetadata as TypeScriptEnum;
            let enumDeclaration = declaration as ts.EnumDeclaration;

            typeMetadata.kind = 'enum';
            typeMetadata.values = [];

            for (let enumValue of enumDeclaration.members) {
                typeMetadata.values.push({
                    name: getPropertyName(enumValue.name)
                });
            }
        }

        else if (declaration.kind === ts.SyntaxKind.PropertySignature) {
            // TODO
        }

        else if (declaration.kind === ts.SyntaxKind.EnumMember) {
            // TODO
        }

        else if (declaration.kind === ts.SyntaxKind.ImportClause) {
            // TODO
        }

        else if (declaration.kind === ts.SyntaxKind.NamespaceImport) {
            // TODO
        }

        else {
            throw new NodeError(`Unsupported type declaration kind: ${declaration.kind}`, declaration);
        }
    }

    if (extendsList) {
        (typeLookup.typeMetadata as TypeScriptInterface).extends = extendsList;
    }

    let existingTypeLookup = typesLookupByName.get(typeLookup.typeMetadata.fullName);

    if (existingTypeLookup) {
        typesLookup.set(symbol.typeId, existingTypeLookup);
        return existingTypeLookup.typeMetadata;
    }

    else {
        typesLookupByName.set(typeLookup.typeMetadata.fullName, typeLookup);
        return typeLookup.typeMetadata;
    }
}

function populateCommentSegments(parts: ts.SymbolDisplayPart[], segments: TypeScriptCommentTextSegment[]) {
    for (let i = 0; i < parts.length; i++) {
        let tagText = parts[i];

        if (tagText.kind === 'link') {
            let linkTag: TypeScriptCommentLinkSegment = {
                kind: 'link',
                text: ''
            }

            i++;

            for (; i < parts.length; i++) {
                let currentLinkTag = parts[i];

                if (currentLinkTag.kind === 'link') {
                    break;
                }

                linkTag.text += currentLinkTag.text;
            }

            linkTag.text = linkTag.text.trim();
            segments.push(linkTag);
        }

        else if (tagText.kind !== 'parameterName') {
            if (tagText.kind !== 'text' || !segments.find(s => s.text === tagText.text)) {
                let actualTagText = tagText.text;

                if (tagText.kind === 'text' && actualTagText.startsWith('{')) {
                    let typeText = actualTagText.substr(0, actualTagText.indexOf('}') + 1);

                    actualTagText = actualTagText.substr(typeText.length).trim();
                    typeText = typeText.substr(1, typeText.length - 2);

                    segments.push({
                        kind: 'type',
                        text: typeText
                    });
                }

                if (tagText.kind !== 'text' || actualTagText) {
                    segments.push({
                        kind: tagText.kind === 'space' || tagText.kind === 'lineBreak' ? 'text' : tagText.kind,
                        text: actualTagText
                    });
                }
            }
        }
    }
}

function getMethodSignatureMetadata(methodSignature: ts.SignatureDeclarationBase, methodName: string, typeChecker: ts.TypeChecker, jsDocTags: ts.JSDocTagInfo[]): TypeScriptMethodSignature {
    let methodSignatureMetadata: TypeScriptMethodSignature = {
        id: codeElementIdCounter++,
        kind: 'signature',
        name: methodName,
        returnType: registerTypeNodeForReflection(methodSignature.type, typeChecker)
    };

    if (methodSignature.parameters) {
        methodSignatureMetadata.parameters = [];

        for (let methodParameter of methodSignature.parameters) {
            let methodParameterMetadata: TypeScriptParameter = {
                kind: 'parameter',
                name: getPropertyName(methodParameter.name),
                type: registerTypeNodeForReflection(methodParameter.type, typeChecker)
            };

            if (methodParameter.questionToken) {
                methodParameterMetadata.flags = {
                    isOptional: true
                }
            }

            if (jsDocTags) {
                for (let jsDocTag of jsDocTags) {
                    if (jsDocTag.name === 'param' && jsDocTag.text.length > 0 && jsDocTag.text[0].kind === 'parameterName' && jsDocTag.text[0].text === methodParameterMetadata.name) {
                        methodParameterMetadata.summary = [];
                        populateCommentSegments(jsDocTag.text, methodParameterMetadata.summary);

                        break;
                    }
                }
            }

            methodSignatureMetadata.parameters.push(methodParameterMetadata);
        }
    }

    if (methodSignature.typeParameters) {
        methodSignatureMetadata.typeParameters = [];

        for (let typeParameter of methodSignature.typeParameters) {
            let typeParameterMetadata: TypeScriptTypeParameter = {
                name: getPropertyName(typeParameter.name)
            };

            if (typeParameter.constraint) {
                typeParameterMetadata.constraint = registerTypeNodeForReflection(typeParameter.constraint, typeChecker);
            }

            methodSignatureMetadata.typeParameters.push(typeParameterMetadata);
        }
    }

    return methodSignatureMetadata;
}

function getJsDocTags(symbol: ts.Symbol, typeChecker: ts.TypeChecker): ts.JSDocTagInfo[] {
    if (!symbol) {
        return null;
    }

    return symbol.getJsDocTags(typeChecker);
}

function getCommentMetadataForMember(member: ts.TypeElement | ts.ClassElement, typeChecker: ts.TypeChecker): TypeScriptCommentMetadata {
    let commentMetadata: TypeScriptCommentMetadata = undefined;
    let summaryComponents: ts.SymbolDisplayPart[] = ts['JsDoc'].getJsDocCommentsFromDeclarations([member], typeChecker);

    if (summaryComponents && summaryComponents.length > 0) {
        if (!commentMetadata) {
            commentMetadata = {};
        }

        commentMetadata.summary = [];
        populateCommentSegments(summaryComponents, commentMetadata.summary);
    }

    else {
        return getCommentMetadata(member.symbol, typeChecker);
    }

    let blockTags: ts.JSDocTagInfo[] = getJsDocTags(member.symbol, typeChecker);

    if (blockTags && blockTags.length > 0) {
        if (!commentMetadata) {
            commentMetadata = {};
        }

        for (let blockTag of blockTags) {
            let target: TypeScriptCommentTextSegment[];

            if (blockTag.name === 'returns') {
                if (!commentMetadata.returns) {
                    commentMetadata.returns = [];
                }

                target = commentMetadata.returns;
            }

            else {
                if (!commentMetadata.blockTags) {
                    commentMetadata.blockTags = [];
                }

                let blockTagMetadata: TypeScriptCommentBlockTag = {
                    tag: '@' + blockTag.name,
                    content: []
                }

                target = blockTagMetadata.content;
                commentMetadata.blockTags.push(blockTagMetadata);
            }

            if (blockTag.text) {
                populateCommentSegments(blockTag.text, target);
            }
        }
    }

    return commentMetadata;
}

function getCommentMetadata(symbol: ts.Symbol, typeChecker: ts.TypeChecker): TypeScriptCommentMetadata {
    let commentMetadata: TypeScriptCommentMetadata = undefined;

    if (symbol) {
        let summaryComponents = symbol.getDocumentationComment(typeChecker);

        if (summaryComponents && summaryComponents.length > 0) {
            if (!commentMetadata) {
                commentMetadata = {};
            }

            commentMetadata.summary = [];
            populateCommentSegments(summaryComponents, commentMetadata.summary);
        }

        let blockTags = symbol.getJsDocTags(typeChecker);

        if (blockTags && blockTags.length > 0) {
            if (!commentMetadata) {
                commentMetadata = {};
            }

            for (let blockTag of blockTags) {
                let target: TypeScriptCommentTextSegment[];

                if (blockTag.name === 'returns') {
                    if (!commentMetadata.returns) {
                        commentMetadata.returns = [];
                    }

                    target = commentMetadata.returns;
                }

                else {
                    if (!commentMetadata.blockTags) {
                        commentMetadata.blockTags = [];
                    }

                    let blockTagMetadata: TypeScriptCommentBlockTag = {
                        tag: '@' + blockTag.name,
                        content: []
                    }

                    target = blockTagMetadata.content;
                    commentMetadata.blockTags.push(blockTagMetadata);
                }

                if (blockTag.text) {
                    populateCommentSegments(blockTag.text, target);
                }
            }
        }
    }

    return commentMetadata;
}

export function registerNodeTypeHandler(nodeType: ts.SyntaxKind, handler: ReflectionHandler) {
    nodeHandlers[nodeType] = handler;
}